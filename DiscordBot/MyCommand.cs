using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    class MyCommand : Attribute
    {
        public MyCommand(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
