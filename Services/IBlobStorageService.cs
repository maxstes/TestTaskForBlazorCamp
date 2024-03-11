using Microsoft.AspNetCore.Components.Forms;
using TestTaskForBlazorCamp.Services.DTO;

namespace TestTaskForBlazorCamp.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFile(string FileName, string contecntType, Stream fileStream);
        string GetBlobSASToken(string FileName);
        Task<bool> SendEmail(FormUploadViewModel model);
    }
}
