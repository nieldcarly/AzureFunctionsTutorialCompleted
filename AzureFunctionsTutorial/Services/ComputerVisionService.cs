using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionsTutorial.Services
{
    public class ComputerVisionService
    {
        static string subscriptionKey { get; set; }

        static string endpoint = "https://mpgallery-screening-service.cognitiveservices.azure.com/";
        static string uriBase = endpoint + "vision/v3.0/analyze";

        public ComputerVisionService(IConfiguration configuration)
        {
            subscriptionKey = configuration.GetConnectionString("ComputerVisionSubscription");
        }

        public async Task<string> MakeAnalysisRequest(string imageURL, ILogger log)
        {
            try
            {
                HttpClient client = new HttpClient();
                log.LogInformation("subscription: " + subscriptionKey);
                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                // The Analyze Image method returns information about the following
                // visual features:
                // Categories:  categorizes image content according to a
                //              taxonomy defined in documentation.
                // Description: describes the image content with a complete
                //              sentence in supported languages.
                // Color:       determines the accent color, dominant color, 
                //              and whether an image is black & white.
                string requestParameters =
                    "visualFeatures=Adult,Categories,Description,Color,Tags";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;
                log.LogInformation("uri: " + uri);

                HttpResponseMessage response;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageURL, log);

                log.LogInformation("Byte Data: " + byteData);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                Console.WriteLine("\nResponse:\n\n{0}\n",
                    JToken.Parse(contentString).ToString());

                return (JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string URL, ILogger log)
        {
            log.LogInformation(URL);
            using (var webClient = new System.Net.WebClient())
            {
                log.LogInformation("here pt ii");
                byte[] imageBytes = webClient.DownloadData(URL);
                log.LogInformation("bytes: " + imageBytes);
                return imageBytes;
            }
        }
    }
}
