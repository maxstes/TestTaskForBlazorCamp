namespace TestTaskForBlazorCamp.Services
{
    public interface IEmailService
    {
        public Task<bool> SendEmail(string recipient, string url);
    }
}
