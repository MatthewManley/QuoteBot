using Domain.Models;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IAudioRepo
    {
        Task<Audio> GetAudioById(uint Id);
        Task<string> GetFileFromPath(string path);
        Task UploadFile(string filePath, string key);
        Task<Audio> CreateAudio(string path, ulong uploader);
    }
}
