using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Models;
using Domain.Repositories;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Aws
{
    public class AuthRepo : IAuthRepo
    {
        private readonly IAmazonDynamoDB dynamoDbClient;

        public AuthRepo(IAmazonDynamoDB dynamoDbClient)
        {
            this.dynamoDbClient = dynamoDbClient;
        }

        public async Task<AuthEntry> CreateAuthEntry(AuthEntry entry)
        {
            var time = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            byte[] key = new byte[32 + time.Length];
            time.CopyTo(key, 0);
            RandomNumberGenerator.Create().GetBytes(key, time.Length, key.Length - time.Length);
            var table = GetAuthTable();
            var doc = Document.FromAttributeMap(new System.Collections.Generic.Dictionary<string, AttributeValue>
            {
                { "key", new AttributeValue{ B = new System.IO.MemoryStream(key) }},
                { "access_token", new AttributeValue{ S = entry.AccessToken }},
                { "expires", new AttributeValue{ N = entry.Expires.ToString() }},
                { "refresh_token", new AttributeValue{ S = entry.RefreshToken }},
                { "scope", new AttributeValue{ S = entry.Scope }},
                { "user_id", new AttributeValue{ N = entry.UserId.ToString() }},
                { "avatar", new AttributeValue { S = entry.Avatar }}
            });
            await table.PutItemAsync(doc);
            return new AuthEntry
            {
                AccessToken = entry.AccessToken,
                Avatar = entry.Avatar,
                Expires = entry.Expires,
                Key = Convert.ToBase64String(key),
                RefreshToken = entry.RefreshToken,
                Scope = entry.Scope,
                UserId = entry.UserId
            };
        }

        public async Task<AuthEntry> GetAuthEntry(string key)
        {
            var table = GetAuthTable();
            var doc = await table.GetItemAsync(new Primitive(new System.IO.MemoryStream(Convert.FromBase64String(key))));
            if (doc == null)
            {
                return null;
            }
            var result = new AuthEntry
            {
                AccessToken = doc["access_token"].AsString(),
                Expires = doc["expires"].AsLong(),
                Key = doc["key"].ToString(),
                RefreshToken = doc["refresh_token"].AsString(),
                Scope = doc["scope"].AsString(),
                UserId = doc["user_id"].AsULong(),
                Avatar = doc["avatar"].AsString()
            };
            return result;
        }

        public async Task DeleteAuthEntry(string key)
        {
            var table = GetAuthTable();
            await table.DeleteItemAsync(new Primitive(new System.IO.MemoryStream(Convert.FromBase64String(key))));
        }

        private Table GetAuthTable()
        {
            return Table.LoadTable(dynamoDbClient, "quotebot-auth");
        }
    }
}
