using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace BulkAdd
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("2 arguments expected");
            }
            var dbPath = args[0];
            var inputAudioDir = args[1];
            var outputAudioDir = args[2];
            var audioFiles = Directory.GetFiles(inputAudioDir);
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = dbPath;
            var sqlConnection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            sqlConnection.Open();
            foreach (var audioFile in audioFiles)
            {
                var file = audioFile;
                var mp3Index = audioFile.IndexOf(".mp3");
                if (mp3Index != -1)
                {
                    file = audioFile.Substring(0, mp3Index);
                }
                var cmd = sqlConnection.CreateCommand();
                var quoteName = file.Split('/').Last();
                var newFileName = Guid.NewGuid().ToString();
                var newPath = Path.Combine(outputAudioDir, newFileName);
                File.Copy(audioFile, newPath);
                cmd.CommandText = "INSERT INTO audio (category, name, path) VALUES ($category, $name, $path);";
                cmd.Parameters.AddWithValue("$category", "halo");
                cmd.Parameters.AddWithValue("$name", quoteName);
                cmd.Parameters.AddWithValue("$path", newFileName);
                cmd.ExecuteNonQuery();
            }
            sqlConnection.Close();
        }
    }
}
