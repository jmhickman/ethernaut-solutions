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


// ABI retrieved by finding correct contract in github, pasting into Remix, compiling.
let targetContractABI = """[{"inputs":[{"internalType":"string","name":"_password","type":"string"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[{"internalType":"string","name":"passkey","type":"string"}],"name":"authenticate","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"getCleared","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"info","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"info1","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[{"internalType":"string","name":"param","type":"string"}],"name":"info2","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"info42","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"infoNum","outputs":[{"internalType":"uint8","name":"","type":"uint8"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"method7123949","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"pure","type":"function"},{"inputs":[],"name":"password","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"theMethodName","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"}]""" |> ABI
let targetContract = loadDeployedContract digest "0xe54d039354078c1e3e23c64c71db9d32f93fd891" RINKEBY targetContractABI |> bindDeployedContract |> List.head
// Apply binds again
let callTarget = makeEthCall web3c constants targetContract
let txnTarget = makeEthTxn web3c constants targetContract


// examine function interface
targetContract.functions |> List.iter (fun p -> printfn $"{p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
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
targetContract.functions
|> List.map (fun f ->
  printfn $"calling {f.name}..."
  callTarget (f |> IndicatedFunction) []
  |> logCallResult)
|> ignore
    

// Well, obviously this was meant to be done in a particular order, but let's follow along. First, call `info2()` with
// the argument 'hello'
callTarget (ByString "info2") [String "hello"]
|> logCallResult
|> ignore


// Well, let's see what happens
txnTarget (ByString "authenticate") [String "ethernaut0"] ZEROV
|> monitorTransaction monitor
|> ignore

callTarget (ByString "getCleared") []
|> logCallResult
|> ignore


// Okay, so now I guess we need to submit this contract address to the original contract
txnEthernaut (ByString "submitLevelInstance") [Address "0xe54d039354078c1e3e23c64c71db9d32f93fd891"] ZEROV
|> monitorTransaction monitor
|> ignore
