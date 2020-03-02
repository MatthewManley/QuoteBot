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

            var bot_key = Environment.GetEnvironmentVariable("bot_key");
            var owner = Environment.GetEnvironmentVariable("owner");
            if (bot_key == null || owner == null)
            {
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
                await client.LoginAsync(TokenType.Bot, bot_key);
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
                .AddSingleton<CommandHandler>()
                .AddTransient(BuildSqliteConnection)
                .AddTransient<IUserRepo, UserRepo>()
                .AddTransient<IAudioRepo, AudioRepo>()
                .AddTransient<InfoModule>()
                .AddTransient<SoundModule>()
                .AddSingleton<StatsService>()
                .BuildServiceProvider();
        }

        private SqliteConnection BuildSqliteConnection(IServiceProvider serviceProvider)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "Db.sqlite";
            return new SqliteConnection(connectionStringBuilder.ConnectionString);
        }
    }
}
