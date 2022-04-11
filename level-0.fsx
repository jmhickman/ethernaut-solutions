#i "path\to\nuget\"
#r "nuget: web3.fs"

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

// What do we see?
deployedEthernaut.functions |> List.iter (fun p -> printfn $"{p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
//createLevelInstance, EVMFunctionHash "0xdfc86b17", EVMFunctionInputs "(address)", Payable
//registerLevel, EVMFunctionHash "0x202023d4", EVMFunctionInputs "(address)", Nonpayable
//renounceOwnership, EVMFunctionHash "0x715018a6", EVMFunctionInputs "()", Nonpayable
//submitLevelInstance, EVMFunctionHash "0xc882d7c2", EVMFunctionInputs "(address)", Nonpayable
//transferOwnership, EVMFunctionHash "0xf2fde38b", EVMFunctionInputs "(address)", Nonpayable
//owner, EVMFunctionHash "0x8da5cb5b", EVMFunctionInputs "()", View



// try and get a level instance, you pass in the address of ( I guess) the base or prototype of the contract, and you
// get a copy, logged via the Event emitted inside `createLevelInstance` 
txnEthernaut (ByString "createLevelInstance") [Address "0x4E73b858fD5D7A5fc1c3455061dE52a53F35d966"] ZEROV
|> monitorTransaction monitor
|> ignore

(*
RPC returned:
{
  "blockHash": "0x0c71c49b3e11c37f9aac84be731c8ba15d1506582fee6677dc2419addd396710",
  "blockNumber": "0xa003ec",
  "contractAddress": null,
  "cumulativeGasUsed": "0xcd714f",
  "effectiveGasPrice": "0x4190ab2f",
  "from": "0x2268b96e204379ee8366505c344ebe5cc34d3a46",
  "gasUsed": "0xc1fa5",
  "logs": [
    {
      "address": "0xd991431d8b033ddcb84dad257f4821e9d5b38c33", 
      "blockHash": "0x0c71c49b3e11c37f9aac84be731c8ba15d1506582fee6677dc2419addd396710",
      "blockNumber": "0xa003ec",
      "data": "0x000000000000000000000000e54d039354078c1e3e23c64c71db9d32f93fd891", // My copy of the contract to interact with
      "logIndex": "0x66",
      "removed": false,
      "topics": [
        "0x7bf7f1ed7f75e83b76de0ff139966989aff81cb85aac26469c18978d86aac1c2",
        "0x0000000000000000000000002268b96e204379ee8366505c344ebe5cc34d3a46"
      ],
      "transactionHash": "0xe31a574efddef1295ad53b96ebf57091b0a5b9d656f3ed72265d3f8b197fa92f",
      "transactionIndex": "0x43",
      "transactionLogIndex": "0x0",
      "type": "mined"
    }
  ],
  "logsBloom": "" //snip
  "status": "0x1",
  "to": "0xd991431d8b033ddcb84dad257f4821e9d5b38c33",
  "transactionHash": "0xe31a574efddef1295ad53b96ebf57091b0a5b9d656f3ed72265d3f8b197fa92f",
  "transactionIndex": "0x43",
  "type": "0x2"
}
*)

// ABI retrieved by finding correct contract in github, pasting into Remix, compiling.
let targetContractABI = """[{"inputs":[{"internalType":"string","name":"_password","type":"string"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[{"internalType":"string","name":"passkey","type":"string"}],"name":"authenticate","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"getCleared","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"info","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"info1","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[{"internalType":"string","name":"param","type":"string"}],"name":"info2","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"info42","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"infoNum","outputs":[{"internalType":"uint8","name":"","type":"uint8"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"method7123949","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"password","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"theMethodName","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"}]""" |> ABI
let targetContract = loadDeployedContract digest "0xe54d039354078c1e3e23c64c71db9d32f93fd891" RINKEBY targetContractABI |> bindDeployedContract |> List.head
// Apply binds again
let callTarget = makeEthCall web3c constants targetContract
let txnTarget = makeEthTxn web3c constants targetContract

// examine function interface
//targetContract.functions |> List.iter (fun p -> printfn $"{p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
//authenticate, EVMFunctionHash "0xaa613b29", EVMFunctionInputs "(string)", Nonpayable
//getCleared, EVMFunctionHash "0x3c848d78", EVMFunctionInputs "()", View
//info, EVMFunctionHash "0x370158ea", EVMFunctionInputs "()", Pure
//info1, EVMFunctionHash "0xd4c3cf44", EVMFunctionInputs "()", Pure
//info2, EVMFunctionHash "0x2133b6a9", EVMFunctionInputs "(string)", Pure
//info42, EVMFunctionHash "0x2cbd79a5", EVMFunctionInputs "()", Pure
//infoNum, EVMFunctionHash "0xc253aebe", EVMFunctionInputs "()", View
//method7123949, EVMFunctionHash "0xf0bc7081", EVMFunctionInputs "()", Pure
//password, EVMFunctionHash "0x224b610b", EVMFunctionInputs "()", View
//theMethodName, EVMFunctionHash "0xf157a1e3", EVMFunctionInputs "()", View


// Since these take no args and are all calls, let's dial everyone
//targetContract.functions
//|> List.map (fun f ->
//  printfn $"calling {f.name}..."
//  callTarget (f |> IndicatedFunction) []
//  |> logCallResult)
//|> ignore
    
// The results were:
//calling authenticate...
//calling getCleared...
//[Bool false]
//calling info...
//[String "You will find what you need in info1()."]
//calling info1...
//[String "Try info2(), but with "hello" as a parameter."]
//calling info2...
//calling info42...
//[String "theMethodName is the name of the next method."]
//calling infoNum...
//[Uint256 "42"]
//calling method7123949...
//[String "If you know the password, submit it to authenticate()."]
//calling password...
//[String "ethernaut0"]
//calling theMethodName...
//[String "The method name is method7123949."]

// Well, obviously this was meant to be done in a particular order, but let's follow along. First, call `info2()` with
// the argument 'hello'
//callTarget (ByString "info2") [String "hello"]
//|> logCallResult
//|> ignore

// [String "The property infoNum holds the number of the next info method to call."]
// The "property" (?!). This isn't javascript bro.
// It seems we inadvertently got the answer because the ABI included pseudo 'getter' functions for the public stored
// values.

// Well, let's see what happens
txnTarget (ByString "authenticate") [String "ethernaut0"] ZEROV
|> monitorTransaction monitor
|> ignore

callTarget (ByString "getCleared") []
|> logCallResult
|> ignore

(*
Beginning monitoring of transaction 0xd7b432d365c45a664406de6f6bb22f97478556052e4a1f322ab6606ec63136b7
RPC returned:
{
  "blockHash": "0xcbf2c00cb70b529a3c5d2970472169b7652eda22b20c7d9817a3e2cb9bff9872",
  "blockNumber": "0xa004c9",
  "contractAddress": null,
  "cumulativeGasUsed": "0x644f71",
  "effectiveGasPrice": "0x3b9aca41",
  "from": "0x2268b96e204379ee8366505c344ebe5cc34d3a46",
  "gasUsed": "0xb801",
  "logs": [],
  "logsBloom": "0x0", //snip
  "status": "0x1",
  "to": "0xe54d039354078c1e3e23c64c71db9d32f93fd891",
  "transactionHash": "0xd7b432d365c45a664406de6f6bb22f97478556052e4a1f322ab6606ec63136b7",
  "transactionIndex": "0x34",
  "type": "0x2"
}
[Bool true]
*)

// Okay, so now I guess we need to submit this contract address to the original contract
txnEthernaut (ByString "submitLevelInstance") [Address "0xe54d039354078c1e3e23c64c71db9d32f93fd891"] ZEROV
|> monitorTransaction monitor
|> ignore

(*
RPC returned:
{
  "blockHash": "0xed3bfb4c09c5b00d2d9ab673b4c70b62aa7f1777154c6345b4f9e64d7e4d705e",
  "blockNumber": "0xa00550",
  "contractAddress": null,
  "cumulativeGasUsed": "0xa4ca1f",
  "effectiveGasPrice": "0x3b9aca31",
  "from": "0x2268b96e204379ee8366505c344ebe5cc34d3a46",
  "gasUsed": "0x99ef",
  "logs": [
    {
      "address": "0xd991431d8b033ddcb84dad257f4821e9d5b38c33",
      "topics": [
        "0x9dfdf7e3e630f506a3dfe38cdbe34e196353364235df33e5a3b588488d9a1e78",
        "0x0000000000000000000000002268b96e204379ee8366505c344ebe5cc34d3a46"
      ],
      "data": "0x0000000000000000000000004e73b858fd5d7a5fc1c3455061de52a53f35d966",
      "blockNumber": "0xa00550",
      "transactionHash": "0x479c7cb410c8cdd22f19cf9bf4fff3b51b12a6b32680cb32826e72182017456b",
      "transactionIndex": "0x40",
      "blockHash": "0xed3bfb4c09c5b00d2d9ab673b4c70b62aa7f1777154c6345b4f9e64d7e4d705e",
      "logIndex": "0x8c",
      "removed": false
    }
  ],
  "logsBloom": "0x0", // snip
  "status": "0x1",
  "to": "0xd991431d8b033ddcb84dad257f4821e9d5b38c33",
  "transactionHash": "0x479c7cb410c8cdd22f19cf9bf4fff3b51b12a6b32680cb32826e72182017456b",
  "transactionIndex": "0x40",
  "type": "0x1"
}

Proof of correct submission: 0x479c7cb410c8cdd22f19cf9bf4fff3b51b12a6b32680cb32826e72182017456b
