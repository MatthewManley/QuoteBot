using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Services;
using Domain.Repos;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
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
        public static async Task Main(string[] args)
        {
            await new Program().Run(args);
        }

        public async Task Run(string[] args)
        {
            var missingSettings = Settings.MissingSettings().ToList();
            if (missingSettings.Count > 0)
            {
                Console.WriteLine("Missing required environment variables: ");
                Console.WriteLine(string.Join(", ", missingSettings));
                return;
            }
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += Log;
                client.Ready += async () =>
                {
                    var channelId = 178546341314691072UL;
                    if (client.GetChannel(channelId) is SocketTextChannel announceChannel)
                    {
                        await announceChannel.SendMessageAsync("I just started up!");
                    }
                    else
                    {
                        Console.WriteLine($"Could not announce to channel {channelId}");
                    }
                };

                var sql = services.GetRequiredService<DbConnection>();
                var userRepo = services.GetRequiredService<IUserRepo>();
                await BuildDb(sql, userRepo);

                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, Settings.BotKey);
                await client.StartAsync();
                services.GetRequiredService<StatsService>().Init();

                // // Here we initialize the logic required to register our commands.
                services.GetRequiredService<CommandHandler>().InitializeAsync();
                await Task.Delay(-1);
            }
        }

        private async Task BuildDb(DbConnection connection, IUserRepo userRepo)
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
        

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddTransient<IDiscordClient>(x => x.GetRequiredService<DiscordSocketClient>())
                .AddSingleton<CommandHandler>()
                .AddTransient<DbConnection>(BuildSqliteConnection)
                .AddTransient<IUserRepo, UserRepo>()
                .AddTransient<IAudioRepo, AudioRepo>()
                .AddTransient<ICategoryRepo, CategoryRepo>()
                .AddTransient<InfoModule>()
                .AddTransient<SoundModule>()
                .AddTransient<AdminModule>()
                .AddSingleton<StatsService>()
                .BuildServiceProvider();
        }

        private SqliteConnection BuildSqliteConnection(IServiceProvider serviceProvider)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = Environment.GetEnvironmentVariable("db_path");
            return new SqliteConnection(connectionStringBuilder.ConnectionString);
        }
    }
}
