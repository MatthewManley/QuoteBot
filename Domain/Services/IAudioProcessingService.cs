using Domain.Models;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IAudioProcessingService
    {
        Task<AudioOwner> Upload(IFormFile formFile, ulong owner, ulong uploader, string name, CancellationToken token = default);
    }
}
