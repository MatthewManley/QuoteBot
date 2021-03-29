using Dapper;
using Domain.Models;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aws
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public CategoryRepo(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Category> GetCategory(uint id)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT id Id, name Name, owner OwnerId FROM category WHERE id = @id;";
            var parameters = new
            {
                id = id
            };
            return await dbConnection.QueryFirstAsync<Category>(cmdText, parameters);
        }

        public async Task<Category> CreateCategory(string name, ulong owner)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "INSERT INTO category (name, owner) VALUES (@name, @owner);";
            var parameters = new
            {
                name = name,
                owner = owner
            };
            await dbConnection.ExecuteAsync(cmdText, parameters);
            var id = await dbConnection.ExecuteScalarAsync<uint>("SELECT LAST_INSERT_ID();");
            return new Category
            {
                Id = id,
                Name = name,
                OwnerId = owner
            };
        }
    }
}
