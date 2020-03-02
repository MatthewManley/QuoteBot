using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DiscordBot.Services
{
    public class StatsService
    {
        private DateTime startTime;
        private ConcurrentQueue<string> historyQueue;
        public StatsService()
        {
            historyQueue = new ConcurrentQueue<string>();
        }

        public void Init()
        {
            startTime = DateTime.Now;
        }

        public TimeSpan GetUptime()
        {
            return DateTime.Now - startTime;
        }

        public void AddToHistory(string person, string quote)
        {
            historyQueue.Enqueue($"!{person} {quote}");
            while (historyQueue.Count > 5)
            {
                if (!historyQueue.TryDequeue(out var _))
                {
                    break;
                }
            }
        }

        public List<string> GetHistory()
        {
            return historyQueue.ToList();
        }
    }
}
