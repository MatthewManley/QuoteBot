using System;
using System.Text.Json.Serialization;

namespace Discord.Responses
{
    public class UserGuild
    {
        [JsonPropertyName("id")]
        public string IdString
        {
            get
            {
                return _id;
            }

            init
            {
                _id = value;
                Id = Convert.ToUInt64(value);
            }
        }

        private string _id { get; init; }

        [JsonIgnore]
        public ulong Id { get; private init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("icon")]
        public string Icon { get; init; }

        [JsonPropertyName("owner")]
        public bool Owner { get; init; }

        [JsonPropertyName("permissions")]
        public string PermissionsString
        {
            get
            {
                return _permissionsString;
            }
            init
            {
                _permissionsString = value;
                var toInt = Convert.ToUInt64(value);
                PermissionsInt = toInt;
            }
        }

        private string _permissionsString { get; set; }

        [JsonIgnore]
        public ulong PermissionsInt { get; init; }

        [JsonPropertyName("features")]
        public string[] Features { get; init; }
    }
}
