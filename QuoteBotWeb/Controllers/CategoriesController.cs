using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using QuoteBotWeb.Models.Categories;
using System.Linq;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IUserService userService;
        private readonly IAudioOwnerRepo audioOwnerRepo;
        private readonly IQuoteBotRepo quoteBotRepo;
        private readonly ICategoryRepo categoryRepo;

        public CategoriesController(IUserService userService,
                                  IAudioOwnerRepo audioOwnerRepo,
                                  IQuoteBotRepo quoteBotRepo,
                                  ICategoryRepo categoryRepo)
        {
            this.userService = userService;
            this.audioOwnerRepo = audioOwnerRepo;
            this.quoteBotRepo = quoteBotRepo;
            this.categoryRepo = categoryRepo;
        }

        [Route("Guild/{server}/Categories")]
        public async Task<IActionResult> Index(ulong server)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var userGuilds = await userService.GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }
            var audio_owners = await audioOwnerRepo.GetAudioOwnersByOwner(server);
            var categories = await quoteBotRepo.GetCategoriesByOwner(server);
            var pairs = await quoteBotRepo.GetAudioCategoriesByOwner(server);
            var paired_quotes = pairs.Join(audio_owners, pair => pair.AudioOwnerId, quote => quote.Id, (pair, quote) => (pair, quote));
            var result = categories.GroupJoin(paired_quotes,
                category => category.Id,
                pair => pair.pair.CategoryId,
                (category, pairs) => (category, pairs.Select(x => x.quote).ToList())
            ).OrderBy(x => x.category.Name).ToList();
            return View(new IndexViewModel
            {
                Categories = result,
                AllAudio = audio_owners.ToList(),
                Server = server
            });
        }

        [HttpGet("Guild/{server}/Categories/Create")]
        public async Task<IActionResult> Create([FromRoute] ulong server)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var userGuilds = await userService.GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost("Guild/{server}/Categories/Create")]
        public async Task<IActionResult> Create([FromRoute] ulong server, [FromForm] string name)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var userGuilds = await userService.GetUserGuilds(authEntry);
            if (!userGuilds.Any(x => x.Id == server))
            {
                return Unauthorized();
            }

            if (!ValidateName(name, out var cleanedName))
            {
                return BadRequest("Invalid category name");
            }

            await categoryRepo.CreateCategory(cleanedName, server);
            return RedirectToAction("Index", new { server = server });
        }

        private bool ValidateName(string name, out string cleaned)
        {
            cleaned = name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            cleaned = cleaned.Trim();
            if (cleaned.Length < 1 || cleaned.Length > 64)
            {
                return false;
            }
            if (!cleaned.All(x => char.IsLetterOrDigit(x) || x == '-' || x == '_' || x == ':'))
            {
                return false;
            }
            return true;
        }

        [HttpGet("Guild/{server}/Categories/Delete/{id}")]
        public async Task<IActionResult> GetDelete([FromRoute] ulong server, [FromRoute] uint id)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            var userGuilds = await userService.GetUserGuilds(authEntry);
            var category = await categoryRepo.GetCategory(id);
            if (!userGuilds.Any(x => x.Id == server) || category.OwnerId != server)
            {
                return Unauthorized();
            }
            return View("Delete", new DeleteViewModel(category, server));
        }

        [HttpPost("Guild/{server}/Categories/Delete/{id}")]
        public async Task<IActionResult> PostDelete([FromRoute] ulong server, [FromRoute] uint id, [FromForm] bool confirm, [FromForm] uint categoryId)
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            if (!confirm || id != categoryId)
            {
                return BadRequest();
            }
            var userGuilds = await userService.GetUserGuilds(authEntry);
            var category = await categoryRepo.GetCategory(id);
            if (category is null || !userGuilds.Any(x => x.Id == server) || category.OwnerId != server)
            {
                return Unauthorized();
            }
            await categoryRepo.DeleteCategory(id);
            return RedirectToAction("Index", new { server = server });
        }
    }
}
