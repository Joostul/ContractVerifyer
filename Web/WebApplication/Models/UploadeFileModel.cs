using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.EthereumHelpers;

namespace WebApplication.Models
{
    public class UploadeFileModel
    {
        public EthereumContractInfo EthereumContractInfo { get; set; }
        public IFormFile File { get; set; }
    }
}
