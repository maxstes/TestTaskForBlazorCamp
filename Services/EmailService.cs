using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;

namespace TestTaskForBlazorCamp.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string connectString;
        private readonly string sender ;
        private readonly EmailClient emailClient;
        private readonly string subject = "Your file access upload!";
        public EmailService(IConfiguration configuration,ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            connectString  = _configuration.GetConnectionString("AzureCommunicationService");
            sender = _configuration.GetSection("Email:Sender").Value;
            emailClient = new EmailClient(connectString);
        }
        public async Task<bool> SendEmail(string recipient,string url)
        {
            try
            {
                string contentEmail = $"<a href={url}>Link your file</a>";
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    Azure.WaitUntil.Completed,
                    sender,
                    recipient,
                    subject,
                    contentEmail);
                EmailSendResult statusMonitor = emailSendOperation.Value;

                if(statusMonitor.Status == Azure.Communication.Email.EmailSendStatus.Succeeded)
                {
                    return true;
                }
                _logger.LogWarning(message: $"Email Sent. Status = {emailSendOperation.Value}");
                return false;
            }
            catch(RequestFailedException ex)
            {
                _logger.LogWarning($"Email send operation failed with error code: {ex.ErrorCode},message: {ex.Message}");
                throw;
            }

        }
    }
}
