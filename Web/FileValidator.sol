pragma solidity ^0.4.24;

contract FileValidation {
    string public sourceContract;

    constructor(string SourceContract) public{
        sourceContract = SourceContract;
    }

    function ValidateFile(string contractToValidate) public view returns (bool){
        return keccak256(bytes(contractToValidate)) == keccak256(bytes(sourceContract));
    }
}