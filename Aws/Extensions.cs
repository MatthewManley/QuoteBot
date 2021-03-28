using Amazon.DynamoDBv2;
using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Amazon.S3;

namespace Aws
{
    public static class Extensions
    {
        private const string V = "key";

        public static IServiceCollection ConfigureAwsServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDefaultAWSOptions(configuration.GetAWSOptions());
            serviceCollection.AddAWSService<IAmazonDynamoDB>();
            serviceCollection.AddAWSService<IAmazonS3>();
            serviceCollection.Configure<DbOptions>(configuration.GetSection("Database"));
            serviceCollection.Configure<S3Options>(configuration.GetSection("S3"));
            serviceCollection.AddTransient<IAuthRepo, AuthRepo>();
            serviceCollection.AddTransient<DbConnectionFactory>();
            serviceCollection.AddTransient<IQuoteBotRepo, QuoteBotRepo>();
            serviceCollection.AddTransient<IAudioOwnerRepo, AudioOwnerRepo>();
            serviceCollection.AddTransient<IAudioRepo, AudioRepo>();
            serviceCollection.AddTransient<IAudioCategoryRepo, AudioCategoryRepo>();
            serviceCollection.AddTransient<ICategoryRepo, CategoryRepo>();
            serviceCollection.AddTransient<IServerRepo, ServerRepo>();
            return serviceCollection;
        }

        public static void AddAuthMiddleware(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var keyCookie = context.Request.Cookies[V];
                context.Items["key"] = null;
                if (keyCookie != null)
                {
                    var authRepo = context.RequestServices.GetRequiredService<IAuthRepo>();
                    var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    AuthEntry authEntry = null;
                    if (!memoryCache.TryGetValue($"key={keyCookie}", out authEntry))
                    {
                        authEntry = await authRepo.GetAuthEntry(keyCookie);
                        if (authEntry != null)
                        {
                            memoryCache.Set($"key={authEntry.Key}", authEntry);
                        }
                    }
                    if (authEntry != null)
                        context.Items["key"] = authEntry;
                }
                await next();
            });
        }


        public static void AddParameterWithValue(this DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            cmd.Parameters.Add(param);
        }

        public static MySqlCommand CreateCommand(this MySqlConnection connection, string commandText, IEnumerable<(string, object)> parameters)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            foreach (var (name, value) in parameters)
                cmd.AddParameterWithValue(name, value);
            return cmd;
        }

        public static async IAsyncEnumerable<T> ReadToEnumerable<T>(this MySqlDataReader reader, Func<MySqlDataReader, T> func)
        {
            while (await reader.ReadAsync())
            {
                yield return func(reader);
            }
        }
    }
}
