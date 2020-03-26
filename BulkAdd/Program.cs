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
        private static AudioOwnerRepo audioOwnerRepo;
        private static DbConnection connection;
        private static AudioCategoryRepo audioCategoryRepo;
        static async Task Main(string[] args)
        {
            var dbPath = "/home/matthew/quotebot-data/db.sqlite";
            // var inputAudioDir = "/home/matthew/Downloads/halo";
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = dbPath;
            connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            audioRepo = new AudioRepo(connection);
            categoryRepo = new CategoryRepo(connection);
            audioOwnerRepo = new AudioOwnerRepo(connection);
            audioCategoryRepo = new AudioCategoryRepo(connection);
            ulong currentServerId = 178546341314691072;
            var currentCategories = await categoryRepo.GetCategories(currentServerId);
            var currentAudioOwners = await audioOwnerRepo.GetAudioOwners(currentServerId);
            var servers = new List<ulong>() { 305020806662979594, 636452095465226250 };
            foreach (var server in servers)
            {
                var serverCategories = new List<Category>();
                foreach (var item in currentCategories)
                {
                    var newCategory = await categoryRepo.CreateCategory(new Category {
                        Name = item.Name,
                        OwnerId = server
                    });
                    serverCategories.Add(newCategory);
                }
                foreach (var audioOwner in currentAudioOwners)
                {
                    var newAudioOwner = await audioOwnerRepo.AddAudioOwner(new AudioOwner
                    {
                        AudioId = audioOwner.AudioId,
                        Name = audioOwner.Name,
                        OwnerId = server
                    });
                    var audioCategories = (await audioCategoryRepo.GetAudioCategoriesByAudioOwnerId(audioOwner.Id)).Select(x => x.CategoryId);
                    foreach (var audioCategoryId in audioCategories)
                    {
                        var firstCategory = currentCategories.First(x => x.Id == audioCategoryId);
                        var newCategory = serverCategories.First(x => x.Name == firstCategory.Name);
                        await audioCategoryRepo.AddAudioCategory(new AudioCategory {
                            AudioOwnerId = newAudioOwner.Id,
                            CategoryId = newCategory.Id
                        });
                    }
                }
            }
        }

        // public static async Task ParseDir(DirectoryInfo dir, Category[] categories)
        // {
        //     var subDirs = dir.GetDirectories();
        //     var files = dir.GetFiles();
        //     foreach (var file in files)
        //     {
        //         await AddFile(file, categories);
        //     }
        //     foreach (var subDir in subDirs)
        //     {
        //         var newCategories = new Category[categories.Length + 1];
        //         Array.Copy(categories, newCategories, categories.Length);
        //         var nextCategory = await categoryRepo.GetCategoryByName(subDir.Name);
        //         if (nextCategory is null)
        //         {
        //             var id = await categoryRepo.CreateCategory(subDir.Name);
        //             nextCategory = new Category
        //             {
        //                 Id = id,
        //                 Name = subDir.Name
        //             };
        //         }
        //         newCategories[newCategories.Length - 1] = nextCategory;
        //         await ParseDir(subDir, newCategories);
        //     }
        // }

        // public static async Task AddFile(FileInfo file, Category[] categories)
        // {
        //     var newPath = Guid.NewGuid().ToString();
        //     var name = file.Name.Substring(0, file.Name.IndexOf(".mp3"));
        //     var audioId = await audioRepo.AddAudio(new Audio
        //     {
        //         Name = name,
        //         Path = newPath
        //     });
        //     foreach (var category in categories)
        //     {
        //         await audioRepo.AddCategoryToAudio(category.Id, audioId.Id);
        //     }
        //     await s3Client.UploadObjectFromFilePathAsync("quotebot-audio", newPath, file.FullName, new Dictionary<string, object>());
        // }
    }
}
