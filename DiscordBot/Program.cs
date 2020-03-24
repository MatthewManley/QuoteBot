using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Options;
using DiscordBot.Services;
using Domain.Repos;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.AddDefaultAWSOptions(configuration.GetAWSOptions())
                        .Configure<BotOptions>(configuration.GetSection("Bot"))
                        .Configure<AuthOptions>(hostContext.Configuration.GetSection("Auth"))
                        .AddSingleton<DiscordSocketClient>()
                        .AddTransient<IDiscordClient>(x => x.GetRequiredService<DiscordSocketClient>())
                        .AddSingleton<Bot>()
                        .AddSingleton<CommandHandler>()
                        .AddTransient<DbConnection>(BuildSqliteConnection)
                        .AddTransient<IUserRepo, UserRepo>()
                        .AddTransient<IAudioRepo, AudioRepo>()
                        .AddTransient<ICategoryRepo, CategoryRepo>()
                        .AddTransient<InfoModule>()
                        .AddTransient<SoundModule>()
                        .AddTransient<AdminModule>()
                        .AddSingleton<StatsService>()
                        .AddAWSService<Amazon.S3.IAmazonS3>()
                        .AddHostedService<Startup>();
                });

        private static SqliteConnection BuildSqliteConnection(IServiceProvider serviceProvider)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = serviceProvider.GetRequiredService<IOptions<BotOptions>>().Value.DatabasePath;
            return new SqliteConnection(connectionStringBuilder.ConnectionString);
        }
    }
}
