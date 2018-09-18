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

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
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
            if(file == null || file.Length == 0)
            {
                ViewBag.Message = "Not a valid file.";
                return RedirectToAction("Index");
            }

            string fileHash;
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
                        return Content(stringBuilder.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Not a valid file.";
                return View();
            }

            return RedirectToAction("Index");
        }
    }
}
