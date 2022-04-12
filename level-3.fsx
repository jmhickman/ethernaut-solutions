#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.4.8" 


open web3.fs

// Values and partial applications
let digest = newKeccakDigest
let web3c = createWeb3Connection "http://127.0.0.1:1248" "2.0"
let constants = createDefaultConstants "0x2268b96e204379ee8366505c344ebe5cc34d3a46"
let constants' = {constants with maxPriorityFeePerGas = Some "0x5d682f00"}
let monitor = createReceiptMonitor web3c

let call = makeEthCall web3c constants'
let txn = makeEthTxn web3c constants'

let ethernautContractAddress = "0xD991431D8b033ddCb84dAD257f4821E9d5b38C33"
let ethernautABI = """[{"inputs":[{"internalType":"contractLevel","name":"_level","type":"address"}],"name":"createLevelInstance","outputs":[],"stateMutability":"payable","type":"function"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"player","type":"address"},{"indexed":false,"internalType":"contractLevel","name":"level","type":"address"}],"name":"LevelCompletedLog","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"player","type":"address"},{"indexed":false,"internalType":"address","name":"instance","type":"address"}],"name":"LevelInstanceCreatedLog","type":"event"},{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"previousOwner","type":"address"},{"indexed":true,"internalType":"address","name":"newOwner","type":"address"}],"name":"OwnershipTransferred","type":"event"},{"inputs":[{"internalType":"contractLevel","name":"_level","type":"address"}],"name":"registerLevel","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"renounceOwnership","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"addresspayable","name":"_instance","type":"address"}],"name":"submitLevelInstance","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"newOwner","type":"address"}],"name":"transferOwnership","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI

// Load up our contract, set deeper partials
let deployedEthernaut = loadDeployedContract digest ethernautContractAddress RINKEBY ethernautABI |> bindDeployedContract |> List.head
let callEthernaut = call deployedEthernaut
let txnEthernaut = txn deployedEthernaut


// Get a level instance, you pass in the address of ( I guess) the base or prototype of the contract, and you
// get a copy, logged via the Event emitted inside `createLevelInstance` 
//txnEthernaut (ByString "createLevelInstance") [Address "0x4dF32584890A0026e56f7535d0f2C6486753624f"] "0"
//|> monitorTransaction monitor
//|> ignore

// deployed to 0x97154c96bd11acbc8d28b0df5405f920d09f8e5e

let level3ABI = """[{"inputs":[],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[],"name":"consecutiveWins","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"bool","name":"_guess","type":"bool"}],"name":"flip","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"}]""" |> ABI
let level3 = loadDeployedContract digest "0x97154c96bd11acbc8d28b0df5405f920d09f8e5e" RINKEBY level3ABI |> bindDeployedContract |> List.head

//partials
let callLevel3 = makeEthCall web3c constants level3 
let txnLevel3 = makeEthTxn web3c constants' level3


//level3.functions
//|> List.iter(fun p -> printfn $"functions: {p.name}, {p.hash}, {p.canonicalInputs}, {p.config}")
//functions: consecutiveWins, EVMFunctionHash "0xe6f334d7", EVMFunctionInputs "()", View
//functions: flip, EVMFunctionHash "0x1d263f67", EVMFunctionInputs "(bool)", Nonpayable
//functions: receive, EVMFunctionHash "0xa3e76c0f", EVMFunctionInputs "()", Payable

// win condition: win 10 times consecutively
// So this looks annoying, but basically I need to 
// poll a block (current block - 1), get its blockhash, and then cast that to a uint256
// then I do division against the 'factor' 57896044618658097711785492504343953926634992332820282019728792003956564819968 to 
// see if the outcome of the integer division is 0 or 1.
// based on the outcome, I have to get the guess into that block

let divisor = 57896044618658097711785492504343953926634992332820282019728792003956564819968I

// Current standing
callLevel3 (ByString "consecutiveWins") []
|> logCallResult

let ethblock =
    makeEthRPCCall web3c EthMethod.BlockNumber []
    |> fun m ->
        match m with
        | CurrentBlock b -> 
            printfn $"current block: {b}"
            b
        | _ -> ""

// Apparently what the EVM thinks is the 'current block' and what the RPC node thinks is the 'current block' don't line up
// When I performed this subtraction, I consistently lost, even on blocks where I was included immediately
// Reverting this subtraction got me mixed results.
//let blockMinusOne = 
//    ethblock 
//    |> strip0x 
//    |> hexToBigIntP 
//    |> fun i -> (i - 1I).ToString() 
//    |> bigintToHex 
//    |> fun p -> p.TrimStart('0')
//    |> prepend0x


// Make the call to get the block by number, get the blockhash out, do the math, and call the `flip()` with the correct guess
makeEthRPCCall web3c EthMethod.GetBlockByNumber [ethblock; "false"]
|> fun p ->
    match p with
    | Block m ->
        let top = m.hash |> strip0x |> hexToBigIntP
        printfn $"dividing {top} by factor..."
        let res = bigint.Divide(top, divisor)
        match res with
        | x when x = 0I -> 
            printfn $"Hit false branch: {res}"
            txnLevel3 (ByString "flip") [Bool false] ZEROV
            |> monitorTransaction monitor
            |> ignore
        | x -> 
            printfn $"Hit true branch: {res}"
            txnLevel3 (ByString "flip") [Bool true] ZEROV
            |> monitorTransaction monitor
            |> ignore
    | x ->
        printfn $"{p}"
        () 
        
// Hopefully +1
callLevel3 (ByString "consecutiveWins") []
|> logCallResult

