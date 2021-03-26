using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace QuoteBotWeb.Models.Components
{
    public class Quote
    {
        public AudioOwner AudioOwner { get; set; }
        public List<Category> Categories { get; set; }
    }
}
