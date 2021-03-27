using AzureFunctionsTutorial.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using AzureFunctionsTutorial.Services;

namespace AzureFunctionsTutorial.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlobService _blobService;
        private QueueService _queueService;

        public HomeController(ILogger<HomeController> logger, BlobService blobService, QueueService queueService)
        {
            _logger = logger;
            _blobService = blobService;
            _queueService = queueService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("photo-upload/{pId}")]
        public async Task<IActionResult> PhotoUpload([FromForm(Name = "file")] IFormFile photo, string pId)
        {
            await _blobService.UploadAsync(photo, pId);
            await _queueService.SendMessageAsync(photo.FileName, "new-images");
            return Ok();
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
    }
}
