using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class Startup : BackgroundService
    {
        public Startup(IConfiguration configuration, IServiceProvider services)
        {
            Configuration = configuration;
            Services = services;
        }

        public IConfiguration Configuration { get; }
        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = Services.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<Bot>();
            await bot.Run();
            await Task.Delay(-1, stoppingToken);
            await bot.Stop();
        }
    }
}