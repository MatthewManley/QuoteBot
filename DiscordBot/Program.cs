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

                var sql = services.GetRequiredService<SqliteConnection>();
                var userRepo = services.GetRequiredService<IUserRepo>();
                await BuildDb(sql, userRepo);

                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                Console.WriteLine("test");
                await client.LoginAsync(TokenType.Bot, Settings.BotKey);
                await client.StartAsync();
                services.GetRequiredService<StatsService>().Init();


                // Here we initialize the logic required to register our commands.
                services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private async Task BuildDb(SqliteConnection connection, IUserRepo userRepo)
        {
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS user_role (user_id TEXT, role TEXT);";
            await cmd.ExecuteNonQueryAsync();

            var cmd2 = connection.CreateCommand();
            cmd2.CommandText = "CREATE TABLE IF NOT EXISTS audio (category TEXT, name TEXT, path TEXT);";
            await cmd2.ExecuteNonQueryAsync();

            await connection.CloseAsync();
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
                .AddTransient(BuildSqliteConnection)
                .AddTransient<IUserRepo, UserRepo>()
                .AddTransient<IAudioRepo, AudioRepo>()
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
