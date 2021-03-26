using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteBotWeb.Models.Home
{
    public class IndexViewModel
    {
        public bool LoggedIn { get; set; }
        public string Avatar { get; set; }
        public ulong? UserId { get; set; }
    }
}
