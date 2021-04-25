using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Models;
using Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

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
        private const string ModeratorRole = "ModeratorRole";
        private readonly IAmazonDynamoDB dynamoDbClient;
        private readonly IOptions<DynamoDbOptions> dynamoDbOptions;

        public ServerRepo(IAmazonDynamoDB dynamoDbClient, IOptions<DynamoDbOptions> dynamoDbOptions)
        {
            this.dynamoDbClient = dynamoDbClient;
            this.dynamoDbOptions = dynamoDbOptions;
        }

        public async Task<ServerConfig> GetServerConfig(ulong serverId)
        {
            var primitiveKey = new Primitive(serverId.ToString(), true);
            var table = GetTable();
            var doc = await table.GetItemAsync(primitiveKey);
            if (doc is null)
            {
                return null;
            }
            var result = DocumentToServerConfig(doc);
            return result;
        }

        public async Task<bool> PutServerConfig(ServerConfig serverConfig)
        {
            var table = GetTable();
            var document = ServerConfigToDocument(serverConfig);
            await table.PutItemAsync(document);
            return true;
        }

        public async Task<string> GetServerPrefix(ulong serverId)
        {
            var config = await GetServerConfig(serverId);
            return config.Prefix;
        }

        private Table GetTable()
        {
            return Table.LoadTable(dynamoDbClient, dynamoDbOptions.Value.ServerConfigTable);
        }

        private static ServerConfig DocumentToServerConfig(Document doc)
        {
            var moderatorRolePrimitive = doc[ModeratorRole].AsPrimitive();
            ulong? moderatorRole = moderatorRolePrimitive?.AsULong();

            var voiceChannelListPrimitive = doc[VoiceChannelList];
            var voiceChannelList = voiceChannelListPrimitive is DynamoDBNull ? new List<ulong>() : voiceChannelListPrimitive.AsListOfPrimitive().Select(x => x.AsULong()).ToList();

            var textChannelListPrimitive = doc[TextChannelList];
            var textChannelList = textChannelListPrimitive is DynamoDBNull ? new List<ulong>() : textChannelListPrimitive.AsListOfPrimitive().Select(x => x.AsULong()).ToList();

            return new ServerConfig
            {
                Prefix = doc[Prefix].AsString(),
                ServerId = doc[ServerId].AsULong(),
                TextChannelList = textChannelList,
                VoiceChannelList = voiceChannelList,
                TextChannelListType = doc[TextChannelListType].AsString(),
                VoiceChannelListType = doc[VoiceChannelListType].AsString(),
                ModeratorRole = moderatorRole
            };
        }

        private static Document ServerConfigToDocument(ServerConfig serverConfig)
        {
            var map = new System.Collections.Generic.Dictionary<string, AttributeValue>
            {
                { ServerId, new AttributeValue{ N = serverConfig.ServerId.ToString() }},
                { TextChannelListType, new AttributeValue{ S = serverConfig.TextChannelListType }},
                { VoiceChannelListType, new AttributeValue{ S = serverConfig.VoiceChannelListType }},
            };

            if (serverConfig.TextChannelList is null || serverConfig.TextChannelList.Count == 0)
                map.Add(TextChannelList, new AttributeValue { NULL = true });
            else
                map.Add(TextChannelList, new AttributeValue { NS = serverConfig.TextChannelList.Select(x => x.ToString()).ToList() });

            if (serverConfig.VoiceChannelList is null || serverConfig.VoiceChannelList.Count == 0)
                map.Add(VoiceChannelList, new AttributeValue { NULL = true });
            else
                map.Add(VoiceChannelList, new AttributeValue { NS = serverConfig.VoiceChannelList.Select(x => x.ToString()).ToList() });

            if (serverConfig.Prefix is null)
                map.Add(Prefix, new AttributeValue { NULL = true });
            else
                map.Add(Prefix, new AttributeValue { S = serverConfig.Prefix });

            if (serverConfig.ModeratorRole.HasValue)
                map.Add(ModeratorRole, new AttributeValue { N = serverConfig.ModeratorRole.ToString() });
            else
                map.Add(ModeratorRole, new AttributeValue { NULL = true });

            var document = Document.FromAttributeMap(map);
            return document;

        }
    }
}
