using Amazon.S3;
using Amazon.S3.Model;
using Dapper;
using Domain.Models;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aws
{
    public class AudioRepo : IAudioRepo
    {
        private readonly IAmazonS3 s3Client;
        private readonly DbConnectionFactory dbConnectionFactory;

        public AudioRepo(IAmazonS3 amazonS3Client, DbConnectionFactory dbConnectionFactory)
        {
            s3Client = amazonS3Client;
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Audio> GetAudioById(uint Id)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "SELECT audio.id Id, audio.path Path, audio.uploader Uploader FROM audio " +
                "WHERE audio.id = @id";
            var parameters = new { id = Id };
            return await dbConnection.QueryFirstAsync<Audio>(cmdText, parameters);
        }

        public async Task<Audio> CreateAudio(string path, ulong uploader)
        {
            using var dbConnection = await dbConnectionFactory.CreateConnection();
            var cmdText = "INSERT INTO audio (path, uploader) VALUES (@path, @uploader);";
            var parameters = new
            {
                path = path,
                uploader = uploader
            };
            await dbConnection.ExecuteAsync(cmdText, parameters);
            var id = await dbConnection.ExecuteScalarAsync<uint>("SELECT LAST_INSERT_ID();");
            return new Audio
            {
                Id = id,
                Path = path,
                Uploader = uploader
            };
        }

        public Task<string> GetFileFromPath(string path)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = "quotebot-audio-post",
                Key = path,
                Expires = DateTime.Now.AddMinutes(1),
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.GET,
            };
            return Task.FromResult(s3Client.GetPreSignedURL(request));
        }

        public async Task UploadFile(string filePath, string key)
        {
            var por = new PutObjectRequest
            {
                FilePath = filePath,
                Key = key,
                ContentType = "audio/ogg",
                BucketName = "quotebot-audio-post",
            };
            var response = await s3Client.PutObjectAsync(por);
        }
    }
}
