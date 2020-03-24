using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBot
{
    public class Startup : BackgroundService
    {
        private readonly IHostEnvironment environment;

        public Startup(IConfiguration configuration, IServiceProvider services, IHostEnvironment environment)
        {
            Configuration = configuration;
            Services = services;
            this.environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = Services.CreateScope())
            {
                var sql = scope.ServiceProvider.GetRequiredService<DbConnection>();
                await BuildDb(sql);
                await scope.ServiceProvider.GetRequiredService<Bot>().Run();
                await Task.Delay(-1, stoppingToken);
            }
        }

        //TODO: Database building should probably get moved into its own section
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
    }
}