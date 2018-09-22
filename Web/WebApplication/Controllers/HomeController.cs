using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using WebApplication.EthereumHelpers;
using Nethereum.Contracts;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEthereumService _service;
        private const string abi = @"[{""inputs"": [{""name"": ""SourceContract"",""type"": ""string""}],""payable"": false,""stateMutability"": ""nonpayable"",""type"": ""constructor""},{""constant"": true,""inputs"": [],""name"": ""sourceContract"",""outputs"": [{""name"": """",""type"": ""string""}],""payable"": false,""stateMutability"": ""view"",""type"": ""function""},{""constant"": true,""inputs"": [{""name"": ""contractToValidate"",""type"": ""string""}],""name"": ""ValidateFile"",""outputs"": [{""name"": """",""type"": ""bool""}],""payable"": false,""stateMutability"": ""view"",""type"": ""function""}]";
        private const string byteCode = "608060405234801561001057600080fd5b506040516103bd3803806103bd83398101604052805101805161003a906000906020840190610041565b50506100dc565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f1061008257805160ff19168380011785556100af565b828001600101855582156100af579182015b828111156100af578251825591602001919060010190610094565b506100bb9291506100bf565b5090565b6100d991905b808211156100bb57600081556001016100c5565b90565b6102d2806100eb6000396000f30060806040526004361061004b5763ffffffff7c0100000000000000000000000000000000000000000000000000000000600035041663a444ae418114610050578063eaef36a6146100da575b600080fd5b34801561005c57600080fd5b50610065610147565b6040805160208082528351818301528351919283929083019185019080838360005b8381101561009f578181015183820152602001610087565b50505050905090810190601f1680156100cc5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b3480156100e657600080fd5b506040805160206004803580820135601f81018490048402850184019095528484526101339436949293602493928401919081908401838280828437509497506101d59650505050505050565b604080519115158252519081900360200190f35b6000805460408051602060026001851615610100026000190190941693909304601f810184900484028201840190925281815292918301828280156101cd5780601f106101a2576101008083540402835291602001916101cd565b820191906000526020600020905b8154815290600101906020018083116101b057829003601f168201915b505050505081565b60008060405180828054600181600116156101000203166002900480156102335780601f10610211576101008083540402835291820191610233565b820191906000526020600020905b81548152906001019060200180831161021f575b50506040519081900381208551909350859250819060208401908083835b602083106102705780518252601f199092019160209182019101610251565b5181516020939093036101000a6000190180199091169216919091179052604051920182900390912093909314959450505050505600a165627a7a72305820f7b1a7fcdfb022ca05968f4082e3549184cf70cc7740b77895414c6bbfd035660029";
        private const int gas = 4700000;

        public HomeController(IEthereumService ethereumService)
        {
            _service = ethereumService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> ImportContract(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Not a valid file.";
                return RedirectToAction("Index");
            }
            var fileContent = GetFileContents(file);

            var contractInfo = new EthereumContractInfo()
            {
                Abi = abi,
                Bytecode = byteCode,
                Name = "ContractVerifyer"
            };

            var transactionHash = await _service.ReleaseContract(contractInfo, gas, fileContent);

            Contract contract = await _service.GetContract(contractInfo);
            var function = contract.GetFunction("ValidateFile");

            try
            {
                var isSameFile = await function.CallAsync<bool>();
            }
            catch (Exception ex)
            {
                throw;
            }


            return RedirectToAction("Index");
        }

        private string GetFileContents(IFormFile file)
        {
            string filePath = Path.GetTempFileName();

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyToAsync(stream).Wait();

                    using (WordprocessingDocument document = WordprocessingDocument.Open(stream, false))
                    {
                        var body = document.MainDocumentPart.Document.Body;
                        var stringBuilder = new StringBuilder();
                        foreach (var item in body)
                        {
                            stringBuilder.Append(item.InnerText);
                        }

                        return stringBuilder.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Not a valid file.";
                return "error";
            }
        }
    }
}
