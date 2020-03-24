using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using quote_bot_web_api.Options;
using quote_bot_web_api.Services;

namespace QuoteBot.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OAuthController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<OAuthController> _logger;
        private readonly IDiscordApi discordApi;

        public OAuthController(ILogger<OAuthController> logger, IDiscordApi discordApi)
        {
            _logger = logger;
            this.discordApi = discordApi;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new BadRequestResult();
            }
            var response = await this.discordApi.AccessToken(code);
            return new OkObjectResult(response);
        }
    }
}
