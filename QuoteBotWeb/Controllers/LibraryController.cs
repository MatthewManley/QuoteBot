using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using QuoteBotWeb.Models;
using QuoteBotWeb.Models.Library;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class LibraryController : Controller
    {
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IAudioRepo audioRepo;

        public LibraryController(IAudioRepo audioRepo, IAudioOwnerRepo audioOwnerRepo, IAudioProcessingService audioProcessingService)
        {
            this.audioRepo = audioRepo;
            this.audioOwnerRepo = audioOwnerRepo;
            this.audioProcessingService = audioProcessingService;
        }

        public async Task<IActionResult> Index()
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var userAudio = await audioOwnerRepo.GetAudioOwnersByOwner(authEntry.UserId);
            return View(new IndexViewModel
            {
                audioOwners = userAudio,
            });
        }

        
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(UploadFileForm uploadFileForm)
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
            var authEntry = HttpContext.GetAuthEntry();

            if (authEntry is null)
            {
                return Redirect("/login");
            }
            await audioProcessingService.Upload(uploadFileForm.File, CancellationToken.None, authEntry.UserId, authEntry.UserId, uploadFileForm.Name);
            return Ok();
        }
    }
}
