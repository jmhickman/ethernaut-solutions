#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// my instance address 0x4DA3fC29b084F6aA313306b329978260E1E53855

// clear condition: read a secret out of block storage, and submit the key to set bool `locked` to false.

// Now, in my mind, there's two ways to do this: 
// Look up the constructor args on a block explorer
// read the storage.

// 1) The storage read from etherscan for the relevant array member was: 0x2cecfc617099e538d30a4feff5fd6626f4d893e48f70a0f2737389e490d60537
// 2) We want the upper 16 bytes of that (pretty sure) so 0x2cecfc617099e538d30a4feff5fd6626

makeEthRPCCall env RINKEBY EthMethod.GetStorageAt ["0x4DA3fC29b084F6aA313306b329978260E1E53855"; "0x5"; LATEST] |> env.log Log
// [+] Value: 0x2cecfc617099e538d30a4feff5fd6626f4d893e48f70a0f2737389e490d60537

let lockedABI = """[{"inputs":[{"internalType":"bytes32[3]","name":"_data","type":"bytes32[3]"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[],"name":"ID","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"locked","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"bytes16","name":"_key","type":"bytes16"}],"name":"unlock","outputs":[],"stateMutability":"nonpayable","type":"function"}]""" |> ABI
let locked =  loadDeployedContract env "0x4DA3fC29b084F6aA313306b329978260E1E53855" RINKEBY lockedABI |> bindDeployedContract |> List.head

txn locked (ByString "unlock") [BytesSz "0x2cecfc617099e538d30a4feff5fd6626"] ZEROV
|> env.log Log

call locked (ByString "locked") [] |> env.log Log
// [+] Call result: [Bool false]
