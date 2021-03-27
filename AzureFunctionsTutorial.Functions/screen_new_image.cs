using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using AzureFunctionsTutorial.Services;
using System.IO;

namespace AzureFunctionsTutorial.Functions
{
    public class screen_new_image
    {
        public ComputerVisionService _computerVisionService;
        public static BlobService _blobService;
        public static QueueService _queueService;

        public screen_new_image(BlobService blobService, ComputerVisionService computerVisionService, QueueService queueService)
        {
            _blobService = blobService;
            _computerVisionService = computerVisionService;
            _queueService = queueService;
        }

        [FunctionName("screen_new_image")]
        public async Task RunAsync([ServiceBusTrigger("new-images", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            JObject queueItem = JObject.Parse(myQueueItem);                 // Parse the object
            string imageName = queueItem["imageid"].ToString();
            string imageURL = _blobService.GetUnscreenedImage(imageName);
            log.LogInformation(imageURL);

            var content = await _computerVisionService.MakeAnalysisRequest(imageURL, log);
            log.LogInformation(content);
            JObject jObj = JObject.Parse(content);
            string isRacy = jObj["adult"]["isRacyContent"].ToString();
            log.LogInformation(isRacy);
            if (isRacy == "False")
            {
                await _blobService.Move(imageName, "local-inbound", "local-review");
                await _queueService.SendMessageAsync(imageName, "images-to-approve");
            }
            else
            {
                _blobService.DeleteFile(imageName, "local-inbound");
            }
        }
    }
}
