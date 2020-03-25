using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Domain.Models;
using Infrastructure;
using Microsoft.Data.Sqlite;

namespace BulkAdd
{
    class Program
    {
        private static IAmazonS3 s3Client;
        private static AudioRepo audioRepo;
        private static CategoryRepo categoryRepo;
        private static DbConnection connection;
        static async Task Main(string[] args)
        {
            var dbPath = "/home/matthew/quotebot-data/db.sqlite";
            var inputAudioDir = "/home/matthew/Downloads/halo";
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = dbPath;
            connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            audioRepo = new AudioRepo(connection);
            categoryRepo = new CategoryRepo(connection);
            s3Client = new AmazonS3Client();
            var dirInfo = new DirectoryInfo(inputAudioDir);
            var haloCategory = await categoryRepo.GetCategoryByName("halo");
            if (haloCategory is null)
            {
                var id = await categoryRepo.CreateCategory("halo");
                haloCategory = new Category {
                    Id = id,
                    Name = "halo"
                };
            }
            await ParseDir(dirInfo, new Category[] { haloCategory });
        }

        public static async Task ParseDir(DirectoryInfo dir, Category[] categories)
        {
            var subDirs = dir.GetDirectories();
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                await AddFile(file, categories);
            }
            foreach (var subDir in subDirs)
            {
                var newCategories = new Category[categories.Length + 1];
                Array.Copy(categories, newCategories, categories.Length);
                var nextCategory = await categoryRepo.GetCategoryByName(subDir.Name);
                if (nextCategory is null)
                {
                    var id = await categoryRepo.CreateCategory(subDir.Name);
                    nextCategory = new Category {
                        Id = id,
                        Name = subDir.Name
                    };
                }
                newCategories[newCategories.Length - 1] = nextCategory;
                await ParseDir(subDir, newCategories);
            }
        }

        public static async Task AddFile(FileInfo file, Category[] categories)
        {
            var newPath = Guid.NewGuid().ToString();            
            var name = file.Name.Substring(0, file.Name.IndexOf(".mp3"));
            var audioId = await audioRepo.AddAudio(new Audio {
                Name = name,
                Path = newPath
            });
            foreach (var category in categories)
            {
                await audioRepo.AddCategoryToAudio(category.Id, audioId.Id);                
            }
            await s3Client.UploadObjectFromFilePathAsync("quotebot-audio", newPath, file.FullName, new Dictionary<string, object>());
        }
    }
}
