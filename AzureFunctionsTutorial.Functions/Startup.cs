using AzureFunctionsTutorial.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsTutorial.Functions
{
   public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<BlobService>();
            builder.Services.AddScoped<ComputerVisionService>();
            builder.Services.AddScoped<QueueService>();
        }
    }
}
