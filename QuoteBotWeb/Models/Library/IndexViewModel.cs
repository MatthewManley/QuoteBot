using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteBotWeb.Models.Library
{
    public class IndexViewModel
    {
        public IEnumerable<AudioOwner> audioOwners { get; init; }
    }
}
