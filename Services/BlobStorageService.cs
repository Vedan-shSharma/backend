using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace edusync_api.Services
{
    public class BlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"];
            _containerName = configuration["AzureStorage:ContainerName"];
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            if (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("Azure Storage connection string or container name is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(fileStream, options);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileName)
        {
             if (string.IsNullOrEmpty(_connectionString) || string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("Azure Storage connection string or container name is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Get a reference to the blob
            var blobClient = containerClient.GetBlobClient(fileName);

            // Delete the blob
            await blobClient.DeleteIfExistsAsync();
        }
    }
} 