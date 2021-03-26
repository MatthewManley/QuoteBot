using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuoteBotWeb.Models
{
    public class UploadFileForm
    {

        [Required]
        [Display(Name="Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; }
    }
}
