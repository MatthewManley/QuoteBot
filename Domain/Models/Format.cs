using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Format
    {
        [JsonPropertyName("format_name")]
        public string FormatName { get; set; }

        [JsonPropertyName("format_long_name")]
        public string FormatLongName { get; set; }

        private string _durationString;

        [JsonPropertyName("duration")]
        public string DurationString
        {
            set
            {
                _durationString = value;
                if (double.TryParse(value, out var duration))
                    Duration = duration;
            }
            get => _durationString;
        }

        [JsonIgnore]
        public double Duration { get; private set; }

        private string _sizeString;

        [JsonPropertyName("size")]
        public string SizeString
        {
            set
            {
                _sizeString = value;
                if (int.TryParse(value, out var size))
                    Size = size;
            }
            get => _sizeString;
        }

        public int Size { get; private set; }
    }
}
