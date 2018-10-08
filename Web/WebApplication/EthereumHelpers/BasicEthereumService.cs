using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Nethereum.Contracts;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication.EthereumHelpers
{
    public class BasicEthereumService : IEthereumService
    {
        private Web3 _web3;
        private string _password;
        public string AccountAddress { get; set; }
        private string _storageAccountConnectionstring;

        private string _directory = Path.Join(Directory.GetCurrentDirectory(), "Save");

        private string _filePath = Path.Join(Directory.GetCurrentDirectory(), "Save", "EthContractInfo.txt");

        public BasicEthereumService(IOptions<EthereumSettings> config)
        {
            _web3 = new Web3("http://localhost:7545");
            AccountAddress = config.Value.EhtereumAccount;
            _password = config.Value.EhtereumPassword;
            _storageAccountConnectionstring = config.Value.StorageAccountConnectionstring;
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
            return await ReleaseContract(contractInfo.ContractName, contractInfo.Abi, contractInfo.Bytecode, gas, constructorParameter);
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

        public async Task<Contract> GetContract(string abi, string contractAddress)
        {
            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, _password, 60);
            if (resultUnlocking)
            {
                return _web3.Eth.GetContract(abi, contractAddress);
            }
            return null;
        }

        public async Task<EthereumContractInfo> TryGetContractAddress(EthereumContractInfo contractInfo)
        {
            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, _password, 60);
            if (resultUnlocking)
            {
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(contractInfo.TransactionHash);

                while (receipt == null)
                {
                    Thread.Sleep(5000);
                    receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(contractInfo.TransactionHash);
                }
                contractInfo.ContractAddress = receipt.ContractAddress;
                return contractInfo;
            }
            return null;
        }

        public async Task SaveContractInfoToTableStorage(EthereumContractInfo contractInfo)
        {
            if (!string.IsNullOrEmpty(contractInfo.ContractAddress))
            {
                contractInfo.RowKey = contractInfo.ContractAddress;
            }
            else
            {
                throw new InvalidOperationException("Can't save a contract without a TransactionHash.");
            }
            CloudStorageAccount account = CloudStorageAccount.Parse(_storageAccountConnectionstring);
            var client = account.CreateCloudTableClient();

            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            TableOperation ops = TableOperation.InsertOrMerge(contractInfo);
            await tableRef.ExecuteAsync(ops);
        }

        public async Task<EthereumContractInfo> GetContractFromTableStorage(string contractAddress)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(_storageAccountConnectionstring);
            var client = account.CreateCloudTableClient();

            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            TableOperation ops = TableOperation.Retrieve<EthereumContractInfo>("contracts", contractAddress);
            var tableResult = await tableRef.ExecuteAsync(ops);
            if (tableResult.HttpStatusCode == 200)
                return (EthereumContractInfo)tableResult.Result;
            else
                return null;
        }

        public async Task<List<EthereumContractInfo>> GetContractsFromTableStorageAsync()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(_storageAccountConnectionstring);
            var client = account.CreateCloudTableClient();
            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            var retrieveAllQuery = new TableQuery<EthereumContractInfo>();
            var query = await tableRef.ExecuteQuerySegmentedAsync(retrieveAllQuery, new TableContinuationToken());
            var contracts = query.Results;

            return contracts;
        }

        public void SaveContractInfoToFile(EthereumContractInfo contractInfo)
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }

            var currentContracts = GetEthereumContractsFromFile();
            currentContracts.Add(contractInfo);

            var json = JsonConvert.SerializeObject(currentContracts);
            File.WriteAllText(_filePath, json);
        }

        public List<EthereumContractInfo> GetEthereumContractsFromFile()
        {
            if (File.Exists(_filePath))
            {
                using (var reader = new StreamReader(_filePath))
                {
                    string json = reader.ReadToEnd();
                    var objects = JsonConvert.DeserializeObject<List<EthereumContractInfo>>(json);
                    return objects;
                }
            }
            else
            {
                return new List<EthereumContractInfo>();
            }
        }

        public EthereumContractInfo GetContractFromFile(string contractAddress)
        {
            var contracts = GetEthereumContractsFromFile();
            return contracts.Where(e => e.ContractAddress == contractAddress).FirstOrDefault();
        }
    }
}
