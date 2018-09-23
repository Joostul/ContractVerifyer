using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.EthereumHelpers
{
    public class EthereumContractInfo : TableEntity
    {
        public string Name { get; set; }
        public string Abi { get; set; }
        public string Bytecode { get; set; }
        public string TransactionHash { get; set; }
        public string ContractAddress { get; set; }

        public EthereumContractInfo()
        {
        }

        public EthereumContractInfo(string name, string abi, string bytecode)
        {
            Name = name;
            Abi = abi;
            Bytecode = bytecode;
            PartitionKey = "contract";
            RowKey = name;
        }
    }
}
