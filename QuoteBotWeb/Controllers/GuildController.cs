﻿using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using QuoteBotWeb.Models;
using QuoteBotWeb.Models.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class GuildController : Controller
    {
        private readonly IQuoteBotRepo quoteBotRepo;
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IUserService userService;

        public GuildController(IQuoteBotRepo quoteBotRepo,
                               IAudioOwnerRepo audioOwnerRepo,
                               IAudioProcessingService audioProcessingService,
                               IUserService userService)
        {
            this.quoteBotRepo = quoteBotRepo;
            this.audioOwnerRepo = audioOwnerRepo;
            this.audioProcessingService = audioProcessingService;
            this.userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await userService.GetUserGuilds(authEntry);
            var viewmodel = new Models.Guild.IndexViewModel
            {
                Guilds = userGuilds
            };
            return View(viewmodel);
        }

        [HttpGet("{controller}/{server}/{action}")]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost("{controller}/{server}/{action}")]
        public async Task<IActionResult> Upload([FromForm]UploadFileForm uploadFileForm, [FromRoute]string server)
        {
            if (!ulong.TryParse(server, out var serverId))
            {
                return BadRequest();
            }
            //var token = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
            var token = CancellationToken.None;
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            await audioProcessingService.Upload(uploadFileForm.File, token, serverId, authEntry.UserId, uploadFileForm.Name);
            return Ok();
        }
    }
}
