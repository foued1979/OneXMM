# OneXMM

## Token functions
Token has the usual IERC20 functions since it is an ERC20 Token. On top of the ERC20 functions, the methods below have been added to the contract.

**addPendingBridgeIn**
### Parameters
    **address** *forUser*   The address of the user bridging in
    **uint64** *fromChainId*    The chain id where bridge is comming from
    **uint128** *amount*    The amount of Tokens to bridge

### Description
Adds a pending bridge in for a user. The pending bridge in is added after a user has requested a bridge out, **and** after the bridge out has been validated by Token owner. If the destination contract doesn't have enough available tokens for the bridge, an "Insufficient balance" error is triggered. The amount of Token to be bridged in is removed from the AvailableTokens:
 `AvailableTokens = SafeMath.sub(AvailableTokens, amount)`


**addValidChainId**
### Parameters
    **uint64** *chainId*    A chain Id to add to the list of valid chain Ids

### Description
Adds a chainId for which a bridge of Token is possible. A chainId is valid if a Token smart contract has been issued on the target chainId.


**allocate**
### Parameters
    **address** *to*        The address of the wallet receiving the Token
    **uint256** *amount*    The amount to be allocated

### Description
Allocates Token to a user (balance of user is increased by amount) and adjusts local variables:
 `AllocatedAmount = SafeMath.add(AllocatedAmount, amount)`
 `AvailableTokens = SafeMath.sub(AvailableTokens, amount)`

### Event
Allocated


**cancelBridgeIn**
### Parameters
    **address** *forUser*   The address of the user for whom bridge in has to be cancelled
    **uint64** *fromChainId*    The origin chain Id from where the bridge was requested

### Description
Cancels a bridge in for a user. The pending amount of Tokens to be bridged is added again to the AvailableTokens:
 `AvailableTokens = SafeMath.add(AvailableTokens, pendingAmt)`


**cancelBridgeOut**
### Parameters
    **address** *forUser*   The address of the user for whom bridge out has to be cancelled
    **uint64** *forChainId*    The destination chain Id

### Description
Cancels a bridge out for a user; particularly if the user doesn't have enough balance to comply with the requested bridge out amount. When bridge out is cancelled, the balance of the user is credited back with the bridge out request amount:
 `_balances[forUser] = SafeMath.add(_balances[forUser], uint256(pendingBridgeOutAmountByChain[forUser][forChainId]))`

### Event
BridgeOutCancelled


**getPendingBridgesInForUser**
### Parameters
    **address** *forUser*   The user address for which we want to see the pending bridges in

### Returns
    **Bridge[]**            The list of pending bridges in

### Description
Returns the list of pending bridges in for a given user.


**getPendingBridgesOutForUser**
### Parameters
    **address** *forUser*   The user address for which we want to see the pending bridges out

### Returns
    **Bridge[]**            The list of pending bridges out

### Description
Returns the list of pending bridges out for a given user.


**getPendingBridgesIn**
### Returns
    **address[]**           List of user addresses

### Description
Returns the list of user addresses for which a bridge to the Token smart contract is pending. The bridge goes out from another chain Id to current contract chain Id. Example: if a bridge is requested between Token on chain 1 and Token on chain 2, this function should return the address of user as pendingBridgeIn on chain 2.


**getPendingBridgesOut**
### Returns
    **address[]**           List of user addresses

### Description
Returns the list of user addresses for which a bridge out from the Token smart contract is pending. The bridge goes out from current contract chain Id to another chain Id. Example: if a bridge is requested between Token on chain 1 and Token on chain 2, this function should return the address of user as pendingBridgeOut on chain 1.


**requestBridgeOut**
### Parameters
    **uint64** *toChainId*  The destination chain Id for the bridge out
    **uint128** *amount*    The amount to bridge

### Description
User can request a bridge out if the balance of user is bigger or equal to amount. If the user doesn't ave enough balance, an "Insufficient balance" error is triggered. Balance of user is decreased by the amount of Token to be bridged; user is added to the list of user to bridge out.

### Event
BridgeOutRequested


**validateBridgeIn**
### Parameters
    **address** *forUser*   The address of the user for whom bridge in is validated
    **uint64** *forChainId*     The origin chain Id

### Description
Validates a bridge in for a user. Balance of user is adjusted as well as the AllocatedAmount:
 `_balances[forUser] = SafeMath.add(_balances[forUser], amtToCredit)`
 `AllocatedAmount = SafeMath.add(AllocatedAmount, amtToCredit)`

### Event
BridgeOutValidated


**validateBridgeOut**
### Parameters
    **address** *forUser*   The address of the user for whom bridge out is validated
    **uint64** *forChainId*     The destination chain Id

### Description
Validates a bridge out for a user. Local variables are adjusted:
 `AllocatedAmount = SafeMath.sub(AllocatedAmount, uint256(amt))`
 `AvailableTokens = SafeMath.add(AvailableTokens, uint256(amt))`

### Event
BridgeOutValidated
