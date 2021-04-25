using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuoteBotWeb.Models;
using QuoteBotWeb.Models.Quotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class QuotesController : Controller
    {
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly IUserService userService;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IQuoteBotRepo quoteBotRepo;
        private readonly ICategoryRepo categoryRepo;
        private readonly IAudioCategoryRepo audioCategoryRepo;

        public QuotesController(IAudioOwnerRepo audioOwnerRepo,
                                IUserService userService,
                                IAudioProcessingService audioProcessingService,
                                IQuoteBotRepo quoteBotRepo,
                                ICategoryRepo categoryRepo,
                                IAudioCategoryRepo audioCategoryRepo)
        {
            this.audioOwnerRepo = audioOwnerRepo;
            this.userService = userService;
            this.audioProcessingService = audioProcessingService;
            this.quoteBotRepo = quoteBotRepo;
            this.categoryRepo = categoryRepo;
            this.audioCategoryRepo = audioCategoryRepo;
        }

        [Route("Guild/{server}/Quotes")]
        public async Task<IActionResult> Index([FromRoute] ulong server)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }
            var quotesTask = audioOwnerRepo.GetAudioOwnersByOwner(server);
            var categoriesTask = quoteBotRepo.GetCategoriesByOwner(server);
            var pairsTask = quoteBotRepo.GetAudioCategoriesByOwner(server);
            await Task.WhenAll(quotesTask, categoriesTask, pairsTask);
            var audio_owners = await quotesTask;
            var categories = await categoriesTask;
            var pairs = await pairsTask;
            var paired_categories = pairs.Join(categories, pair => pair.CategoryId, category => category.Id, (pair, category) => (pair, category)); ;
            var result = audio_owners.GroupJoin(
                paired_categories,
                audio_owner => audio_owner.Id, paired_categories => paired_categories.pair.AudioOwnerId,
                (quote, pairs) => (quote, pairs.Select(x => x.category).OrderBy(x => x.Name).ToList())
            ).OrderBy(x => x.quote.Name).ToList();
            return View(new Models.Quotes.IndexViewModel(result, server));
        }


        [HttpGet("Guild/{server}/Quotes/{quote}")]
        public async Task<IActionResult> Edit([FromRoute] ulong server, [FromRoute] uint quote)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            var named_audio = await quoteBotRepo.GetNamedAudioByAudioOwnerId(quote);

            if (named_audio.AudioOwner.OwnerId != server)
            {
                return BadRequest();
            }

            var server_categories = await quoteBotRepo.GetCategoriesByOwner(server);
            var pairs = await quoteBotRepo.GetAudioCategoriesByOwner(server);

            var pair_category_join = pairs.Join(server_categories, pair => pair.CategoryId, cateogry => cateogry.Id, (pair, category) => new { pair, category });
            var in_category = pair_category_join.Where(x => x.pair.AudioOwnerId == named_audio.AudioOwner.Id).Select(x => x.category).ToList();
            var not_in_category = server_categories.Where(x => !in_category.Select(c => c.Id).Contains(x.Id)).ToList();

            var viewModel = new Models.Quotes.EditViewModel(named_audio, in_category, not_in_category, server, quote);
            return View(viewModel);
        }

        [HttpGet("Guild/{server}/Quotes/Upload")]
        public async Task<IActionResult> Upload([FromRoute] ulong server)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            var categories = await categoryRepo.GetCategoriesByOwner(server);
            return View(new UploadViewModel(categories.OrderBy(x => x.Name).ToList()));
        }

        [HttpPost("Guild/{server}/Quotes/Upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string name, [FromRoute] ulong server, [FromForm] List<uint> categories)
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            if (!IsValidName(name, out var cleanedName))
            {
                return BadRequest("Invalid quote name");
            }

            var audio_owner = await audioProcessingService.Upload(file, server, authEntry.UserId, cleanedName, token);
            foreach (var categoryId in categories)
            {
                var category = await categoryRepo.GetCategory(categoryId);
                if (category != null && category.OwnerId == server)
                {
                    await audioCategoryRepo.Create(audio_owner.Id, category.Id);
                }
            }
            return RedirectToAction("Index", new { server });
        }

        private static bool IsValidName(string name, out string cleaned)
        {
            cleaned = name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            cleaned = cleaned.Trim();
            if (cleaned.Length < 1 || cleaned.Length > 64 || cleaned.Any(x => char.IsWhiteSpace(x)))
            {
                return false;
            }
            //if (!cleaned.All(x => char.IsLetterOrDigit(x) || x == '-' || x == '_' || x == ':'))
            //{
            //    return false;
            //}
            return true;
        }


        [HttpGet("Guild/{server}/Quotes/{quote}/Delete")]
        public async Task<IActionResult> Delete([FromRoute] ulong server, [FromRoute] uint quote)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            var named_audio = await quoteBotRepo.GetNamedAudioByAudioOwnerId(quote);

            if (named_audio.AudioOwner.OwnerId != server)
            {
                return BadRequest();
            }
            var viewModel = new DeleteViewModel(named_audio, server);
            return View(viewModel);
        }

        [HttpPost("Guild/{server}/Quotes/{quote}/Delete")]
        public async Task<IActionResult> Delete([FromRoute] ulong server, [FromRoute] uint quote, [FromForm] bool confirm, [FromForm] uint quoteId, [FromQuery] string redirect)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            if (!confirm || quote != quoteId)
            {
                return BadRequest();
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            var named_audio = await quoteBotRepo.GetNamedAudioByAudioOwnerId(quote);
            if (named_audio is null || named_audio.AudioOwner.OwnerId != server)
            {
                return BadRequest();
            }
            await audioOwnerRepo.Delete(quote);
            if (string.IsNullOrWhiteSpace(redirect))
            {
                return RedirectToAction("Index", new { server });
            }
            else
            {
                return LocalRedirect(redirect);
            }
        }

        [HttpPost("Guild/{server}/Quotes/{quote}/Rename")]
        public async Task<IActionResult> Rename([FromRoute] ulong server, [FromRoute] uint quote, [FromForm(Name = "quoteId")] uint audioOwnerId, [FromForm] string name)
        {
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            if (quote != audioOwnerId)
            {
                return BadRequest();
            }

            var userGuilds = await userService.GetAllowedUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            var named_audio = await quoteBotRepo.GetNamedAudioByAudioOwnerId(quote);
            if (named_audio is null || named_audio.AudioOwner.OwnerId != server)
            {
                return BadRequest();
            }

            if (!IsValidName(name, out var cleaned_name))
            {
                return BadRequest("Invalid quote name");
            }

            await audioOwnerRepo.Rename(quote, cleaned_name);

            return RedirectToAction("Edit", new { server, quote });
        }
    }
}
