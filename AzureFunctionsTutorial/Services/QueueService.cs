using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsTutorial.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AzureFunctionsTutorial.Services
{
    public class QueueService
    {
        public string ServiceBusConnectionString { get; set; }
        static IQueueClient queueClient;

        public QueueService(IConfiguration configuration)
        {
            ServiceBusConnectionString = configuration.GetConnectionString("ServiceBusConnection");

        }

        public async Task SendMessageAsync(string fileName, string QueueName)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            try
            {
                ImageInfo newImage = new ImageInfo { imageId = fileName, currentdate = DateTime.Now };
                // Create a new message to send to the queue
                string messageBody = JsonConvert.SerializeObject(newImage);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                // Write the body of the message to the console
                Console.WriteLine($"Sending message: {messageBody}");

                // Send the message to the queue
                await queueClient.SendAsync(message);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
