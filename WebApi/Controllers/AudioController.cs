using System;
using System.Threading.Tasks;
using Domain.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace QuoteBot.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AudioController : ControllerBase
    {
        private readonly ILogger<AudioController> logger;
        private readonly IAudioRepo audioRepo;

        public AudioController(ILogger<AudioController> logger, IAudioRepo audioRepo)
        {
            this.logger = logger;
            this.audioRepo = audioRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAudio()
        {
            var audio = await audioRepo.GetAllAudio();
            return new OkObjectResult(audio);
        }
    }
}