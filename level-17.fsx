#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

let deployedABI = """[{"inputs":[{"internalType":"addresspayable","name":"_to","type":"address"}],"name":"destroy","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"_to","type":"address"},{"internalType":"uint256","name":"_amount","type":"uint256"}],"name":"transfer","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"string","name":"_name","type":"string"},{"internalType":"address","name":"_creator","type":"address"},{"internalType":"uint256","name":"_initialSupply","type":"uint256"}],"stateMutability":"nonpayable","type":"constructor"},{"stateMutability":"payable","type":"receive"},{"inputs":[{"internalType":"address","name":"","type":"address"}],"name":"balances","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"name","outputs":[{"internalType":"string","name":"","type":"string"}],"stateMutability":"view","type":"function"}]""" |> ABI
let token = 
    loadDeployedContract env "0x6076551458AFeB7432B46A00b9445dd922AA48ee" RINKEBY deployedABI
    |> bindDeployedContract
    |> List.head

call token (ByString "name") [] |> env.log Log

(*
  This was...odd. There's no 'losing' contract addresses on a blockchain. They aren't lost so long as you have access to the wallet that created it
  (or even just its address) and a block explorer...
  So yeah, I just found it, called its self-destruct, and got the eth and moved on. Very strange lesson. The blurb is cool that it talks about storing
  Eth this way, but yeah.
*)
