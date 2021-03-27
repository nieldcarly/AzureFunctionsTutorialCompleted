using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsTutorial.Services
{
    public class BlobService
    {
        public string ConnectionString { get; set; }
        public readonly CloudBlobClient _client;

        public BlobService(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("BlobStorageConnection");
            var account = CloudStorageAccount.Parse(ConnectionString);
            _client = account.CreateCloudBlobClient();
        }

        public async Task UploadAsync(IFormFile coverArt, string pId)
        {
            if (coverArt != null)
            {
                //create a container 
                CloudBlobContainer _container = _client.GetContainerReference("local-inbound");
                //create a container if it is not already exists
                if (await _container.CreateIfNotExistsAsync())
                {
                    await _container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                }
                string imageName = coverArt.FileName;

                //get Blob reference

                CloudBlockBlob cloudBlockBlob = _container.GetBlockBlobReference($"/{imageName}");
                cloudBlockBlob.Properties.ContentType = coverArt.ContentType;

                using (var stream = coverArt.OpenReadStream())
                {
                    await cloudBlockBlob.UploadFromStreamAsync(stream);
                }

            }

        }

        public async Task<List<IListBlobItem>> ListBlobsAsync(BlobContinuationToken currentToken)
        {
            CloudBlobContainer _container = _client.GetContainerReference("global-images");
            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await _container.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);
            return results;
        }

        public List<string> GetGlobalImages(BlobContinuationToken currentToken)
        {
            CloudBlobContainer _container = _client.GetContainerReference("global-images");
            var blobsInfoList = new List<string>();
            var blobs = ListBlobsAsync(currentToken).Result; // Use ListBlobsSegmentedAsync for containers with large numbers of files

            foreach (var item in blobs)
            {
                if (item is CloudBlockBlob blob)
                {
                    blobsInfoList.Add(blob.Uri.ToString());
                }
            }
            return blobsInfoList;
        }

        public string GetUnscreenedImage(string name)
        {
            CloudBlobContainer _container = _client.GetContainerReference("local-inbound");
            // Retrieve reference to a blob by filename, e.g. "photo1.jpg".
            var blob = _container.GetBlockBlobReference($"/{name}");
            return blob.Uri.ToString();
        }

        public async Task<CloudBlockBlob> Move(string srcBlobName, string srcContainer, string destContainer)
        {
            CloudBlobContainer src = _client.GetContainerReference(srcContainer);
            CloudBlobContainer dest = _client.GetContainerReference(destContainer);

            CloudBlockBlob blob = src.GetBlockBlobReference($"/{srcBlobName}");
            CloudBlockBlob destBlob;

            await dest.CreateIfNotExistsAsync();

            //Copy source blob to destination container
            destBlob = dest.GetBlockBlobReference($"/{srcBlobName}");
            await destBlob.StartCopyAsync(blob);
            //remove source blob after copy is done.
            await blob.DeleteAsync();
            return destBlob;
        }

        public void DeleteFile(string fileName, string container)
        {
            CloudBlobContainer _container = _client.GetContainerReference(container);

            var blob = _container.GetBlockBlobReference($"/{fileName}");
            blob.DeleteIfExistsAsync();
        }
    }
}
