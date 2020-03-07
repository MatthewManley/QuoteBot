using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Services
{
    public static class Settings
    {
        private const string BotKeyName = "bot_key";
        private const string DbPathName = "db_path";
        private const string AudioPathName = "audio_path";
        private const string OwnerName = "owner";
        private const string TempPathName = "temp_path";
        private const string PrefixName = "prefix";
        private const string RecentCountName = "recent_count";
        private const int RecentCountDefault = 5;

        public static IEnumerable<string> MissingSettings()
        {
            if (BotKey is null)
            {
                yield return BotKeyName;
            }
            if (DbPath is null)
            {
                yield return DbPathName;
            }
            if (AudioPath is null)
            {
                yield return AudioPathName;
            }
            if (TempPath is null)
            {
                yield return TempPathName;
            }
        }

        public static string BotKey
        {
            get
            {
                return Environment.GetEnvironmentVariable(BotKeyName);
            }
        }

        public static string DbPath
        {
            get
            {
                return Environment.GetEnvironmentVariable(DbPathName);
            }
        }

        public static string AudioPath
        {
            get
            {
                return Environment.GetEnvironmentVariable(AudioPathName);
            }
        }

        public static string TempPath
        {
            get
            {
                return Environment.GetEnvironmentVariable(TempPathName);
            }
        }

        public static ulong? Owner
        {
            get
            {
                var str = Environment.GetEnvironmentVariable(OwnerName);
                if (ulong.TryParse(str, out var result))
                {
                    return result;
                }
                return null;
            }
        }

        public static string Prefix
        {
            get
            {
                var val = Environment.GetEnvironmentVariable(PrefixName);
                return string.IsNullOrWhiteSpace(val) ? "!" : val;
            }
        }

        public static int RecentCount
        {
            get
            {
                var val = Environment.GetEnvironmentVariable(RecentCountName);
                if (val is null || !int.TryParse(val, out var result) || result < 0)
                    return RecentCountDefault;
                return result;
            }
        }
    }
}
