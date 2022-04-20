pragma solidity ^0.6.0;

contract ForceReceive {

    receive() external payable {}

    function forceReceive(address payable _f) public {
        selfdestruct(_f);
    }
}
