using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Domain.Models;

namespace DiscordBot.Services
{
    public class StatsService
    {
        private DateTime startTime;
        private ConcurrentQueue<Audio> historyQueue;
        public StatsService()
        {
            historyQueue = new ConcurrentQueue<Audio>();
        }

        public void Init()
        {
            startTime = DateTime.Now;
        }

        public TimeSpan GetUptime()
        {
            return DateTime.Now - startTime;
        }

        public void AddToHistory(Audio audio)
        {
            historyQueue.Enqueue(audio);
            while (historyQueue.Count > 5)
            {
                if (!historyQueue.TryDequeue(out var _))
                {
                    break;
                }
            }
        }

        public List<Audio> GetHistory()
        {
            return historyQueue.ToList();
        }
    }
}
