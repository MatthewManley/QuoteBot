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
        public static async Task Main(string[] args)
        {
            await new Program().Run(args);
        }

        public async Task Run(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            using var services = ConfigureServices();

            var sql = services.GetRequiredService<DbConnection>();
            await BuildDb(sql);

            // Start the bot
            await services.GetRequiredService<Bot>().Run();
            await Task.Delay(-1);
        }

        private async Task BuildDb(DbConnection connection)
        {
            await connection.OpenAsync();

            var getVersionCmd = connection.CreateCommand();
            getVersionCmd.CommandText = "PRAGMA user_version;";
            var user_version = (long)await getVersionCmd.ExecuteScalarAsync();
            Console.WriteLine($"DB version: {user_version}");

            var migrations = GetMigrations().OrderBy(x => x.Item1);
            foreach (var (version, file) in migrations)
            {
                if (user_version >= version)
                {
                    Console.WriteLine($"Skipping migration: {version}");
                    continue;
                }
                Console.WriteLine($"Performing Migration: {version}");
                using var reader = file.OpenText();
                var content = await reader.ReadToEndAsync();

                var migrationCmd = connection.CreateCommand();
                migrationCmd.CommandText = content;
                await migrationCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Performed Migration: {version}");
            }

            await connection.CloseAsync();
        }

        private IEnumerable<(long, FileInfo)> GetMigrations()
        {
            var migrationsDirectory = new DirectoryInfo("Migrations");
            if (!migrationsDirectory.Exists)
            {
                throw new Exception("Migrations folder does not exist");
            }
            var migrationFiles = migrationsDirectory.GetFiles();
            foreach (var file in migrationFiles)
            {
                var fileName = file.Name.Split('.');
                if (fileName.Length != 2)
                    continue;
                if (fileName[1] != "sql")
                    continue;
                if (!long.TryParse(fileName[0], out var num))
                    continue;
                yield return (num, file);
            }
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddDefaultAWSOptions(Configuration.GetAWSOptions())
                .Configure<AuthOptions>(Configuration.GetSection("Auth"))
                .Configure<BotOptions>(Configuration.GetSection("Bot"))
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
                .BuildServiceProvider();
        }

        private SqliteConnection BuildSqliteConnection(IServiceProvider serviceProvider)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = serviceProvider.GetRequiredService<IOptions<BotOptions>>().Value.DatabasePath;
            return new SqliteConnection(connectionStringBuilder.ConnectionString);
        }
    }
}
