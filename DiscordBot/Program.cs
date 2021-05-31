using Amazon.Extensions.NETCore.Setup;
using Aws;
using Discord;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Services;
using Domain.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
                    Console.WriteLine("environment: " + environment.EnvironmentName);
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
                        .AddHttpClient()
                        .AddTransient<DbConnectionFactory>()
                        .AddTransient<InfoModule>()
                        .AddTransient<SoundModule>()
                        .AddSingleton<StatsService>()
                        .AddTransient<AdminModule>()
                        .AddMemoryCache()
                        .AddSingleton<JoinService>()
                        .ConfigureAwsServices(configuration)
                        .AddHostedService<Startup>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<WebStartup>();
                });
    }
}
