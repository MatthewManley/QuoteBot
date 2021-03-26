using System;

namespace Domain.Models
{
    public class UserGuild
    {
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

        public ulong Id { get; private init; }

        public string Name { get; init; }

        public string Icon { get; init; }

        public bool Owner { get; init; }

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

        public ulong PermissionsInt { get; init; }

        public string[] Features { get; init; }
    }
}
