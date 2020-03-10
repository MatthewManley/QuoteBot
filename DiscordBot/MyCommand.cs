using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    class MyCommand : Attribute
    {
        public MyCommand(string name, string permission = null)
        {
            Name = name.ToLower();
            Permission = permission;
        }

        public string Name { get; }
        public string Permission { get;  }
    }
}
