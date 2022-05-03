#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.5" 

open web3.fs

// Values and partial applications
let env = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// my instance: 0xb7d6569C067a2cf05dBf76FF3908Eb8FF3130cc8

let engineABI = """[{"inputs":[],"name":"horsePower","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"initialize","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"newImplementation","type":"address"},{"internalType":"bytes","name":"data","type":"bytes"}],"name":"upgradeToAndCall","outputs":[],"stateMutability":"payable","type":"function"},{"inputs":[],"name":"upgrader","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI
let engine =  loadDeployedContract env "0xb7d6569C067a2cf05dBf76FF3908Eb8FF3130cc8" RINKEBY engineABI |> bindDeployedContract |> List.head

call engine (ByString "upgrader") []
|> env.log Log

makeEthRPCCall env RINKEBY EthMethod.GetStorageAt ["0xb7d6569C067a2cf05dBf76FF3908Eb8FF3130cc8"; "0x360894a13ba1a3210667c828492db98dca3e2076cc3735a920a3ca505d382bbc"; LATEST] |> env.log Log

// using the above, I was able to confirm that the implementationw was located at 0x7cED39691CaFeD5CC337E8231a7f53F7E34d81F9
// It was uninitialized, and so I called the initialize(), not really understanding what I was doing, but really 'just to see'.
// Reading the local variables from the implementation showed my address as the 'upgrader', but there was no change in the proxy contract.
// I couldn't understand why that was, even though I had skimmed through the high points of the transparent proxy info. I forgot though, that
// part of the point of this pattern is that storage changes in the logic don't overwrite storage in the proxy. 

//Duh.

// However, I didn't realize that right away, so I proceeded to mess around with logic upgrades, settling on a simple contract 
// that only had a self-destruct function. Did the upgrade with the bytestring corresponding to the selfdestruct() and the required
// payment address. I honestly didn't think it would work, but when I checked 0x7cED39691CaFeD5CC337E8231a7f53F7E34d81F9 afterwards, 
// the block explorer showed it was destroyed, and the proxy broke (since its logic died). Only after that all happened did I remember
// the nuance about storage.
