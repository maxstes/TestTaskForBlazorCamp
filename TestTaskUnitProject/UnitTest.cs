using Azure.Storage.Blobs;
using TestTaskForBlazorCamp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestTaskUnitProject
{
    public class UnitTest
    {
        private string? blobContainerName;
        private string? blobStorageConnection;

        private string? filename;
        private string? category;

        readonly BlobStorageService? blobStorageService;

        readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        readonly ILogger<BlobStorageService> loggerBlobs = NullLoggerFactory.Instance.CreateLogger<BlobStorageService>();
        readonly ILogger<EmailService> loggerEmails = NullLoggerFactory.Instance.CreateLogger<EmailService>();

        EmailService? emailService;

        readonly Stream stream = new MemoryStream();

        public UnitTest()
        {
            blobStorageService = new BlobStorageService(configuration, emailService!, loggerBlobs);

            InitializationData();
        }
       
        [Fact]
        public async Task Upload_Blobs()
        {
            var result = await blobStorageService!.UploadFile(filename!,category!,stream);
            Assert.IsType<string>(result);            
        }
        [Fact]
        public void CheckConfigurationConnectionContainer()
        {
            var container = new BlobContainerClient(blobStorageConnection, blobContainerName);
            var result = container.Name;
            Assert.IsType<string>(result );
        }
        [Fact]
        public async Task CheckConfigurationEmailConnection()
        {
            string yourEmail = configuration.GetSection("Email:TestEmail").Value;
            var result =  await emailService!.SendEmail(yourEmail,"Test");
            Assert.True(result);
        }
        [Fact]
        public void CheckBlobSasToken()
        {
            var result = blobStorageService!.GetBlobSASToken(filename!);
            Assert.IsType<string>(result);
        }
        private void InitializationData()
        {
            emailService = new(configuration,loggerEmails);

            blobContainerName = configuration.GetSection("AzureStorage:BlobContainerName").Value;
            blobStorageConnection = configuration.GetConnectionString("AzureStorageAccount");

             filename = "2023 Aflac Open Enrollment.docx";
             category = "\"application/vnd.openxmlformats-officedocument.wordprocessingml.document\"";
        }
    }
}
