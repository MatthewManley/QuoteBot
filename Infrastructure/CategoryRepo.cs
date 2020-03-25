using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Repos;
using Microsoft.Data.Sqlite;

namespace Infrastructure
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly DbConnection connection;

        public CategoryRepo(DbConnection connection)
        {
            this.connection = connection;
        }

        public async Task<Category> CreateCategory(Category category)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO category (name, owner) VALUES ($name, $owner); " +
                "SELECT last_insert_rowid();";
            cmd.AddParameterWithValue("$name", category.Name);
            cmd.AddParameterWithValue("$owner", category.OwnerId.ToString());
            var result = (long)await cmd.ExecuteScalarAsync();
            category.Id = result;
            await connection.CloseAsync();
            return category;
        }

        public async Task<List<Category>> GetCategories(ulong ownerId)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT category.id, category.name, category.owner FROM category " +
                "WHERE category.owner = $ownerId;";
            cmd.AddParameterWithValue("$ownerId", ownerId.ToString());
            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new Category
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                OwnerId = ownerId
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        public async Task<List<Category>> GetCategoriesWithAudio(ulong ownerId)
        {
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT category.id, category.name, category.owner FROM category " +
                "WHERE category.owner = $ownerId AND EXISTS(SELECT category FROM audio_category WHERE category.id = audio_category.category);;";
            cmd.AddParameterWithValue("$ownerId", ownerId.ToString());
            using var reader = await cmd.ExecuteReaderAsync();
            var enumerable = reader.ReadToEnumerable(() => new Category
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                OwnerId = ownerId
            });
            var result = await enumerable.ToListAsync();
            await connection.CloseAsync();
            return result;
        }

        // public async Task<Category> GetCategoryById(long id)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText = "SELECT id, name FROM category WHERE id = $id LIMIT 1;";
        //     cmd.AddParameterWithValue("$id", id);

        //     using var reader = await cmd.ExecuteReaderAsync();

        //     var result = await reader.ReadFirstOrDefault(() => new Category
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1)
        //     });

        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<Category> GetCategoryByName(string name)
        // {
        //     await connection.OpenAsync();
        //     using var cmd = connection.CreateCommand();
        //     cmd.CommandText = "SELECT id, name FROM category WHERE name = $name LIMIT 1;";
        //     cmd.AddParameterWithValue("$name", name.ToLowerInvariant());
        //     using var reader = await cmd.ExecuteReaderAsync();

        //     var result = await reader.ReadFirstOrDefault(() => new Category
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1)
        //     });

        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<long> CreateCategory(string name)
        // {
        //     await connection.OpenAsync();
        //     var cmd = connection.CreateCommand();
        //     cmd.CommandText = "INSERT INTO category (name) VALUES ($name); SELECT last_insert_rowid();";
        //     cmd.AddParameterWithValue("$name", name.ToLowerInvariant());
        //     var result = (long)await cmd.ExecuteScalarAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<List<Category>> GetAllCategories()
        // {
        //     await connection.OpenAsync();
        //     var cmd = connection.CreateCommand();
        //     cmd.CommandText = "SELECT id, name FROM category;";
        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var enumerable = reader.ReadToEnumerable(() => new Category
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1)
        //     });
        //     var result = await enumerable.ToListAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<List<Category>> GetAllCategoriesWithAudio()
        // {
        //     await connection.OpenAsync();
        //     var cmd = connection.CreateCommand();
        //     cmd.CommandText = "SELECT id, name FROM category WHERE EXISTS(SELECT category FROM audio_category WHERE category.id = audio_category.category);";
        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var enumerable = reader.ReadToEnumerable(() => new Category
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1)
        //     });
        //     var result = await enumerable.ToListAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }

        // public async Task<List<Category>> GetAllCategoriesWithNoAudio()
        // {
        //     await connection.OpenAsync();
        //     var cmd = connection.CreateCommand();
        //     cmd.CommandText = "SELECT id, name FROM category WHERE NOT EXISTS(SELECT category FROM audio_category WHERE category.id = audio_category.category);";
        //     using var reader = await cmd.ExecuteReaderAsync();
        //     var enumerable = reader.ReadToEnumerable(() => new Category
        //     {
        //         Id = reader.GetInt64(0),
        //         Name = reader.GetString(1)
        //     });
        //     var result = await enumerable.ToListAsync();
        //     await connection.CloseAsync();
        //     return result;
        // }
    }
}