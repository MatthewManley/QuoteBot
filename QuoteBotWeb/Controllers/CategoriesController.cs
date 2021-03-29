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

        public IActionResult Index()
        {
            var authEntry = (AuthEntry)HttpContext.Items["key"];
            if (authEntry is null)
            {
                return Redirect("/login");
            }
            return RedirectToAction("Index", "Guild");
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

        [HttpPost("Guild/{server}/Category/{action}")]
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

            if (string.IsNullOrWhiteSpace(name) || name.Length < 4 || !name.All(x => char.IsLetterOrDigit(x) || x == '_' || x == '-'))
            {
                return BadRequest();
            }

            await categoryRepo.CreateCategory(name, server);
            return RedirectToAction("Index", new { server = server });
        }
    }
}
