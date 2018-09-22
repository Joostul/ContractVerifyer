using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.EthereumHelpers
{
    public interface IEthereumService
    {
        string AccountAddress { get; set; }
        Task<decimal> GetBalance(string address);
        Task<string> ReleaseContract(string name, string abi, string byteCode, int gas);
        Task<string> ReleaseContract(EthereumContractInfo contractInfo, int gas);
        Task<Contract> GetContract(EthereumContractInfo contractInfo);
    }
}
