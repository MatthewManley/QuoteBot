using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IAudioProcessingService
    {
        Task Upload(IFormFile formFile, CancellationToken token, ulong owner, ulong uploader, string name);
    }
}
