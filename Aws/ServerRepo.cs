using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Aws
{
    public class ServerRepo : IServerRepo
    {
        private const string Prefix = "Prefix";
        private const string ServerId = "ServerId";
        private const string TextChannelList = "TextChannelList";
        private const string VoiceChannelList = "VoiceChannelList";
        private const string TextChannelListType = "TextChannelListType";
        private const string VoiceChannelListType = "VoiceChannelListType";
        private readonly IAmazonDynamoDB dynamoDbClient;

        public ServerRepo(IAmazonDynamoDB dynamoDbClient)
        {
            this.dynamoDbClient = dynamoDbClient;
        }

        public async Task<ServerConfig> GetServerConfig(ulong serverId)
        {
            var primitiveKey = new Primitive(serverId.ToString());
            var table = GetTable();
            var doc = await table.GetItemAsync(primitiveKey);
            if (doc is null)
            {
                return null;
            }
            var result = new ServerConfig
            {
                Prefix = doc[Prefix].AsString(),
                ServerId = doc[ServerId].AsULong(),
                TextChannelList = doc[TextChannelList].AsListOfPrimitive().Select(x => x.AsULong()).ToList(),
                VoiceChannelList = doc[VoiceChannelList].AsListOfPrimitive().Select(x => x.AsULong()).ToList(),
                TextChannelListType = doc[TextChannelListType].AsString(),
                VoiceChannelListType = doc[VoiceChannelListType].AsString(),
            };
            return result;
        }

        public async Task<bool> PutServerConfig(ServerConfig serverConfig)
        {
            var table = GetTable();
            var map = new System.Collections.Generic.Dictionary<string, AttributeValue>
            {
                { ServerId, new AttributeValue{ N = serverConfig.ServerId.ToString() }},
                { TextChannelListType, new AttributeValue{ S = serverConfig.TextChannelListType }},
                { VoiceChannelListType, new AttributeValue{ S = serverConfig.VoiceChannelListType }},
                { TextChannelList, new AttributeValue{ NS = serverConfig.TextChannelList.Select(x => x.ToString()).ToList() }},
                { VoiceChannelList, new AttributeValue{ NS = serverConfig.VoiceChannelList.Select(x => x.ToString()).ToList() }},
            };

            if (serverConfig.Prefix is null)
                map.Add(Prefix, new AttributeValue { NULL = true });
            else
                map.Add(Prefix, new AttributeValue{ S = serverConfig.Prefix });

            var document = Document.FromAttributeMap(map);
            var doc = await table.UpdateItemAsync(document);
            return true;
        }

        public async Task<string> GetServerPrefix(ulong serverId)
        {
            var config = await this.GetServerConfig(serverId);
            return config.Prefix;
        }

        private Table GetTable()
        {
            return Table.LoadTable(dynamoDbClient, "quotebot-server");
        }
    }
}
