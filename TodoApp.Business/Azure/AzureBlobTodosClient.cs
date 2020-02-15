using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Business.Azure
{
    public class AzureBlobTodosClient
    {
        private readonly BlobServiceClient serviceClient;
        private readonly string containerName;
        private BlobContainerClient blobContainer { get => serviceClient.GetBlobContainerClient(containerName); }

        public AzureBlobTodosClient(string connectionString, string containerName)
        {
            this.serviceClient = new BlobServiceClient(connectionString);
            this.containerName = containerName;
        }

        public async Task<BlobClient> UploadFileAsync(string fileName, Stream stream)
        {
            await blobContainer.UploadBlobAsync(fileName, stream);
            var blob = blobContainer.GetBlobClient(fileName);

            return blob;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            await blobContainer.GetBlobClient(fileName).DeleteIfExistsAsync();
        }
    }
}
