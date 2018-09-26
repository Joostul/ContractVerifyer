using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.EthereumHelpers
{
    public class EthereumContractInfo : TableEntity
    {
        public string ContractName { get; set; }
        public string Abi { get; set; }
        public string Bytecode { get; set; }
        public string TransactionHash { get; set; }
        public string ContractAddress { get; set; }
        public string FileName { get; set; }

        public EthereumContractInfo()
        {
        }

        public EthereumContractInfo(string contractName, string abi, string bytecode, string fileName)
        {
            ContractName = contractName;
            Abi = abi;
            Bytecode = bytecode;
            PartitionKey = "contracts";
            FileName = fileName;
        }
    }
}
