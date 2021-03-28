using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discord
{
    public static class Extensions
    {
        public static IServiceCollection ConfigureDiscordServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<DiscordOptions>(configuration.GetSection("Discord"));
            serviceCollection.AddHttpClient<IDiscordHttp, DiscordHttp>();
            return serviceCollection;
        }

        public static async Task<T> ReadAsObjectAsync<T>(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
    }
}
