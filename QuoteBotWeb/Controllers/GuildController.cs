using Domain.Models;
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
        private readonly IDiscordHttp discordHttp;
        private readonly IMemoryCache memoryCache;
        private readonly IQuoteBotRepo quoteBotRepo;
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly IAudioProcessingService audioProcessingService;

        public GuildController(IDiscordHttp discordHttp,
                               IMemoryCache memoryCache,
                               IQuoteBotRepo quoteBotRepo,
                               IAudioOwnerRepo audioOwnerRepo,
                               IAudioProcessingService audioProcessingService)
        {
            this.discordHttp = discordHttp;
            this.memoryCache = memoryCache;
            this.quoteBotRepo = quoteBotRepo;
            this.audioOwnerRepo = audioOwnerRepo;
            this.audioProcessingService = audioProcessingService;
        }

        public async Task<IActionResult> Index()
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }

            var userGuilds = await GetUserGuilds(authEntry);
            var viewmodel = new Models.Guild.IndexViewModel
            {
                Guilds = userGuilds
            };
            return View(viewmodel);
        }

        //TODO: should be a service
        private async Task<List<UserGuild>> GetUserGuilds(AuthEntry authEntry)
        {
            var guilds = await memoryCache.GetOrCreateAsync($"userguilds={authEntry.UserId}", async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
                return await discordHttp.GetCurrentUserGuilds(authEntry.AccessToken);
            });
            var validGuilds = guilds.Where(x => x.Owner || (x.PermissionsInt & (uint)Permissions.Administrator) == (uint)Permissions.Administrator).ToList();
            return validGuilds;
        }

        [Route("{controller}/{server}/{action}")]
        public async Task<IActionResult> Categories(string server)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            if (!ulong.TryParse(server, out var serverId))
            {
                return BadRequest();
            }
            var userGuilds = await GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == serverId))
            {
                return Unauthorized();
            }
            var audio_owners = await audioOwnerRepo.GetAudioOwnersByOwner(serverId);
            var categories = await quoteBotRepo.GetCategoriesByOwner(serverId);
            var pairs = await quoteBotRepo.GetAudioCategoriesByOwner(serverId);
            var paired_quotes = pairs.Join(audio_owners, pair => pair.AudioOwnerId, quote => quote.Id, (pair, quote) => (pair, quote));
            var result = categories.GroupJoin(paired_quotes,
                category => category.Id,
                pair => pair.pair.CategoryId,
                (category, pairs) => (category, pairs.Select(x => x.quote).ToList())
            ).OrderBy(x => x.category.Name).ToList();
            return View(new Models.Guild.CategoriesViewModel
            {
                Categories = result,
                AllAudio = audio_owners.ToList()
            });
        }

        [Route("{controller}/{server}/{action}")]
        public async Task<IActionResult> Quotes(string server)
        {

            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            if (!ulong.TryParse(server, out var serverId))
            {
                return BadRequest();
            }
            var userGuilds = await GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == serverId))
            {
                return Unauthorized();
            }
            var quotesTask = audioOwnerRepo.GetAudioOwnersByOwner(serverId);
            var categoriesTask = quoteBotRepo.GetCategoriesByOwner(serverId);
            var pairsTask = quoteBotRepo.GetAudioCategoriesByOwner(serverId);
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
            return View(new Models.Guild.QuotesViewModel
            {
                Quotes = result,
                Server = serverId
            });
        }

        [HttpGet("{controller}/{server}/Quotes/{quote}")]
        public async Task<IActionResult> EditQuote(string server, string quote)
        {
            if (!ulong.TryParse(server, out var serverId))
            {
                return BadRequest();
            }
            if (!uint.TryParse(quote, out var quoteId))
            {
                return BadRequest();
            }
            var authEntry = HttpContext.GetAuthEntry();
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var named_audio = await quoteBotRepo.GetNamedAudioByAudioOwnerId(quoteId);

            if (named_audio.AudioOwner.OwnerId != serverId)
            {
                return BadRequest();
            }

            var server_categories = await quoteBotRepo.GetCategoriesByOwner(serverId);
            var pairs = await quoteBotRepo.GetAudioCategoriesByOwner(serverId);

            var pair_category_join = pairs.Join(server_categories, pair => pair.CategoryId, cateogry => cateogry.Id, (pair, category) => new { pair = pair, category = category });
            var in_category = pair_category_join.Where(x => x.pair.AudioOwnerId == named_audio.AudioOwner.Id).Select(x => x.category).ToList();
            var not_in_category = server_categories.Where(x => !in_category.Select(c => c.Id).Contains(x.Id)).ToList();

            var viewModel = new EditQuoteViewModel
            {
                InCategories = in_category,
                NamedAudio = named_audio,
                NotInCategories = not_in_category,
                Server = server,
                Quote = quote
            };
            return View(viewModel);
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
