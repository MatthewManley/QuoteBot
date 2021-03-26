using Amazon.Extensions.NETCore.Setup;
using Aws;
using Discord;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Services;
using Domain.Options;
using Domain.Repositories;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBot
{
    public class Program
    {
        public IConfiguration Configuration;
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    var environment = hostContext.HostingEnvironment;
                    builder.AddSystemsManager($"/QuoteBot/{environment.EnvironmentName}/");
                    if (environment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var environmentName = hostContext.HostingEnvironment.EnvironmentName;
                    var configuration = hostContext.Configuration;
                    services.AddDefaultAWSOptions(configuration.GetAWSOptions())
                        .Configure<BotOptions>(configuration.GetSection("Bot"))
                        .Configure<AuthOptions>(configuration.GetSection("Auth"))
                        .Configure<DbOptions>(configuration.GetSection("Database"))
                        .AddSingleton<DiscordSocketClient>()
                        .AddTransient<IDiscordClient>(x => x.GetRequiredService<DiscordSocketClient>())
                        .AddSingleton<Bot>()
                        .AddSingleton<CommandHandler>()
                        .AddTransient<DbConnectionFactory>()
                        .AddTransient<InfoModule>()
                        .AddTransient<SoundModule>()
                        .AddSingleton<StatsService>()
                        .AddTransient<IQuoteBotRepo, QuoteBotRepo>()
                        .AddTransient<IServerRepo, FakeServerRepo>()
                        .AddAWSService<Amazon.S3.IAmazonS3>()
                        .AddHostedService<Startup>();
                });
    }
}
