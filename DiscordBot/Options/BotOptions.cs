using System;

namespace DiscordBot.Options
{
    public class BotOptions
    {
        public string DatabasePath { get; set; }
        public string Owner { get; set; }
        public string TempPath { get; set; }

        public string Prefix { get; set; } = "!";

        public string RecentCount { get; set; }
        private const int RecentCountDefault = 5;

        public int RecentCountValue
        {
            get
            {

                if (string.IsNullOrWhiteSpace(RecentCount))
                    return RecentCountDefault;
                if (int.TryParse(RecentCount, out var result))
                    return result;
                throw new Exception("Invalid value for RecentCount configuration");
            }
        }
        public ulong? OwnerValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Owner))
                    return null;
                if (ulong.TryParse(Owner, out var result))
                    return result;
                throw new Exception("Invalid value for Owner configuration");
            }
        }
    }
}