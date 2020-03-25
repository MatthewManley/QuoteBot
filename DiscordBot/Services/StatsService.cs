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
        private ulong seenMessages;
        // private ConcurrentQueue<Audio> historyQueue;
        private ConcurrentDictionary<ulong, ConcurrentQueue<HistoryEntry>> serverHistories = new ConcurrentDictionary<ulong, ConcurrentQueue<HistoryEntry>>();

        public void Init()
        {
            startTime = DateTime.Now;
            seenMessages = 0;
        }

        public TimeSpan GetUptime()
        {
            return DateTime.Now - startTime;
        }

        public void AddToHistory(ulong serverId, HistoryEntry entry)
        {
            var historyQueue = serverHistories.GetOrAdd(serverId, _ => new ConcurrentQueue<HistoryEntry>());
            historyQueue.Enqueue(entry);
            while (historyQueue.Count > 5)
            {
                if (!historyQueue.TryDequeue(out var _))
                {
                    break;
                }
            }
        }

        public void SawMessage()
        {
            seenMessages++;
        }

        public ulong GetSeenMessages()
        {
            return seenMessages;
        }

        public List<HistoryEntry> GetHistory(ulong serverId)
        {
            if( serverHistories.TryGetValue(serverId, out var queue))
            {
                return queue.AsEnumerable().Reverse().ToList();
            }
            return new List<HistoryEntry>();
        }
    }
}
