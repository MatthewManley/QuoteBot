using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace QuoteBotWeb.Components
{
    public class QuoteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(AudioOwner audioOwner, List<Category> categories)
        {
            return View(new Models.Components.Quote
            {
                AudioOwner = audioOwner,
                Categories = categories
            });
        }
    }
}
