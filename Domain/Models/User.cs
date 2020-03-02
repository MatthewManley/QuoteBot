using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class User
    {
        public ulong Id { get; set; }
        public List<string> Roles { get; set; }
    }
}
