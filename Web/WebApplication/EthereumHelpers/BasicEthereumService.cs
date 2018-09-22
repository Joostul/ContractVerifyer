﻿using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Nethereum.Contracts;
using Nethereum.Web3;
using System;
using System.Threading.Tasks;

namespace WebApplication.EthereumHelpers
{
    public class BasicEthereumService : IEthereumService
    {
        private Web3 _web3;
        private string _password;
        public string AccountAddress { get; set; }

        public BasicEthereumService(IOptions<EthereumSettings> config)
        {
            _web3 = new Web3("http://localhost:7545");
            AccountAddress = config.Value.EhtereumAccount;
            _password = config.Value.EhtereumPassword;
        }

        public async Task<decimal> GetBalance(string address)
        {
            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(balance.Value, 18);
        }


        public async Task<string> ReleaseContract(string name, string abi, string byteCode, int gas, string constructorParameter)
        {
            try
            {
                var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, _password, 60);
                if (resultUnlocking)
                {
                    return await _web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, AccountAddress, new Nethereum.Hex.HexTypes.HexBigInteger(gas), constructorParameter);
                }
            }
            catch (Exception ex)
            {
                return "error";
            }
            return "error";
        }

        public async Task<string> ReleaseContract(EthereumContractInfo contractInfo, int gas, string constructorParameter)
        {
            return await ReleaseContract(contractInfo.Name, contractInfo.Abi, contractInfo.Bytecode, gas, constructorParameter);
        }

        public async Task<Contract> GetContract(EthereumContractInfo contractInfo)
        {
            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, _password, 60);
            if (resultUnlocking)
            {
                return _web3.Eth.GetContract(contractInfo.Abi, contractInfo.ContractAddress);
            }
            return null;
        }
    }
}