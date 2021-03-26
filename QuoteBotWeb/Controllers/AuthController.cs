using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace QuoteBotWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IDiscordHttp discordHttp;
        private readonly IAuthRepo authRepo;
        private readonly IMemoryCache memoryCache;

        public AuthController(IDiscordHttp discordHttp, IAuthRepo authRepo, IMemoryCache memoryCache)
        {
            this.discordHttp = discordHttp;
            this.authRepo = authRepo;
            this.memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("login")]
        public IActionResult Login()
        {
            var stateBytes = new byte[16];
            RandomNumberGenerator.Create().GetBytes(stateBytes);
            var state = Base64UrlEncoder.Encode(stateBytes);
            Response.Cookies.Append("state", state, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddMinutes(10),
                IsEssential = true
            });
            var deleteCookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(-100),
                HttpOnly = true,
                IsEssential = true
            };
            if (Request.Cookies.ContainsKey("key"))
                Response.Cookies.Append("key", "", deleteCookieOptions);
            if (Request.Cookies.ContainsKey("username"))
                Response.Cookies.Append("username", "", deleteCookieOptions);
            if (Request.Cookies.ContainsKey("avatar"))
                Response.Cookies.Append("avatar", "", deleteCookieOptions);
            var uri = discordHttp.GetRedirectUri(state);
            return Redirect(uri);
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var deleteCookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-100),
                HttpOnly = true,
                IsEssential = true
            };
            if (Request.Cookies.ContainsKey("key"))
            {
                var key = Request.Cookies["key"];
                memoryCache.Remove($"key={key}");
                Response.Cookies.Append("key", "", deleteCookieOptions);
                await authRepo.DeleteAuthEntry(key);
            }
            if (Request.Cookies.ContainsKey("state"))
                Response.Cookies.Append("state", "", deleteCookieOptions);
            return RedirectToAction("Index", "Home");
        }

        private IReadOnlyList<ulong> allowedUsers = new List<ulong>() {
            107649869665046528, // matt
            304785107821002762, // jimmy
            173512340837367808, // milk
            218600945372758016, // saxton
        }; 

        [Route("oauth")]
        public async Task<IActionResult> OAuth(string code = null, string state = null)
        {
            var now = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(code))
                return BadRequest();

            if (string.IsNullOrWhiteSpace(state))
                return BadRequest();

            var trysKey = $"oauthtrys={Request.HttpContext.Connection.RemoteIpAddress}";
            var trys = (int?)memoryCache.Get(trysKey);
            if (trys.HasValue)
            {
                memoryCache.Set(trysKey, trys.Value + 1, now.AddMinutes(1));
                if (trys.Value > 5)
                {
                    return StatusCode(420, $"Too Many Requests. Try again after {now.AddSeconds(70).ToString("o")}");
                }
            }
            else
            {
                memoryCache.Set(trysKey, 1, TimeSpan.FromMinutes(1));
            }

            var storedState = Request.Cookies["state"];
            if (storedState != state)
                return BadRequest();
            Response.Cookies.Append("state", "", new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddDays(-100),
                IsEssential = true
            });

            var accessTokenResponse = await discordHttp.GetAccessToken(code);

            if (!accessTokenResponse.Scopes.Contains("identify") ||
                !accessTokenResponse.Scopes.Contains("guilds"))
                return BadRequest();

            var userReponse = await discordHttp.GetCurrentUser(accessTokenResponse.AccessToken);
            
            if (!ulong.TryParse(userReponse.Id, out var userId))
            {
                return BadRequest();
            }

            if (!allowedUsers.Contains(userId))
            {
                return Unauthorized();
            }

            var unixTime = now.AddSeconds(accessTokenResponse.ExpiresIn).ToUnixTime();
            var authEntry = new AuthEntry
            {
                AccessToken = accessTokenResponse.AccessToken,
                Expires = unixTime,
                RefreshToken = accessTokenResponse.RefreshToken,
                Scope = accessTokenResponse.Scope,
                UserId = userId,
                Avatar = userReponse.Avatar
            };
            var newAuthEntry = await authRepo.CreateAuthEntry(authEntry);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.FromUnixTimeSeconds(unixTime),
                HttpOnly = true,
                IsEssential = true,
            };
            Response.Cookies.Append("key", newAuthEntry.Key, cookieOptions);
            memoryCache.Set($"key={newAuthEntry.Key}", authEntry, TimeSpan.FromMinutes(1));
            return RedirectToAction("Index", "Home");
        }
    }
}
