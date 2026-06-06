using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ExamenAzure.Services
{
    public interface IBlobService
    {
        Task<string> UploadVideoAsync(IFormFile file);
        string GenerateSasToken(string fileName);
    }
}
