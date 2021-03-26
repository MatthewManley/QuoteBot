using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class FfprobeRoot
    {
        [JsonPropertyName("format")]
        public Format Format { get; set; }
    }
}
