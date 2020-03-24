using System.Threading.Tasks;

namespace QuoteBot.WebApi.Services
{
    public interface IDiscordApi
    {
        Task<string> AccessToken(string code);
    }
}