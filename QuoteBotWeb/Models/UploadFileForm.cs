using Microsoft.AspNetCore.Http;

namespace QuoteBotWeb.Models
{
    public class UploadFileForm
    {

        public string Name { get; set; }

        public IFormFile File { get; set; }
    }
}
