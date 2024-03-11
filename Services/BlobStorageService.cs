using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using TestTaskForBlazorCamp.Services.DTO;

namespace TestTaskForBlazorCamp.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly string blobStorageConnection = string.Empty;
        private readonly string blobContainerName;
        
        public BlobStorageService(IConfiguration configuration, IEmailService emailService,ILogger<BlobStorageService> logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            blobStorageConnection = _configuration.GetConnectionString("AzureStorageAccount");
            blobContainerName = _configuration.GetSection("AzureStorage:BlobContainerName").Value;
        }
        public async Task<string> UploadFile(string fileName, string contecntType, Stream fileStream)
        {
            try
            {
                fileName = RemoveSpacesFromFileName(fileName);
                var container = new BlobContainerClient(blobStorageConnection, blobContainerName);
                var createResponse = await container.CreateIfNotExistsAsync();
                if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                {
                    await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                }
                var blob = container.GetBlobClient(fileName);
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contecntType });
                var urlString = blob.Uri.ToString();
                return urlString;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(message: ex.ToString());
                throw;
            }
        }
        public string GetBlobSASToken(string FileName)
        {
            try
            {
                int tokenLifeTime = 60;
                var azureStorageAccount = _configuration.GetSection("AzureStorage:AzureAccount").Value;
                var azureStorageAccessKey = _configuration.GetSection("AzureStorage:AccessKey").Value;
                Azure.Storage.Sas.BlobSasBuilder blobSasBuilder = new()
                {
                    BlobContainerName = blobContainerName,
                    BlobName = FileName,
                    ExpiresOn = DateTime.UtcNow.AddMinutes(tokenLifeTime),
                };
                blobSasBuilder.SetPermissions(Azure.Storage.Sas.BlobAccountSasPermissions.Read);
                var sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(azureStorageAccount
                    , azureStorageAccessKey)).ToString();
                return sasToken;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(message: ex.ToString());
                throw;
            }
        }
        public async Task<bool> SendEmail(FormUploadViewModel model)
        {
            var sasToken = GetBlobSASToken(model.FileName!);
            string fileUrl = model.FileStorageUrl + "?" + sasToken;
            var result = await _emailService.SendEmail(model.Email!, fileUrl);
            return result;
        }
        private static string RemoveSpacesFromFileName(string fileName)
        {
            return fileName.Replace(" ", "");
        }
    }
}
