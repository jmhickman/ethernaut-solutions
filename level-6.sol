// This level was completed via Remix, because I didn't have Fallback calling code in Web3.fs at the time I performed this stage.

//So, delegated calls are essentially like a a weird implicit import statement and method call. In effect, as long as the target contract
//uses the same value names for whatever it does or works with as the calling contract, it can manipulate those values as though the called
//function existed in the calling contract.

// So the idea is to invoke the `pwn()` function in the target delegation call to change the value of the 'local' `owner`
function pwn() {owner = msg.sender;}

// Interestingly, the `msg.sender` and other properties of the msg struct aren't proxied by the calling contract. That seems like something that would
// have to be done manually? This might be what the 'proxy pattern' is, for all I know at this stage.

// So all I needed was to get the function selector of `pwn` and put that into a call that would interact with the fallback. Web3.fs doesn't have that
// forced fallback call yet, so I just used Remix. `0xdd365b8b` was the hash, and that's all it took (aside from the gas issue that made the calls look 
// like they succeeded, but weren't really. Etherscan even said "This ran out of gas but still completed successfully" in yellow text. Lying junk.
