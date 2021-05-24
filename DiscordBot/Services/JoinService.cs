using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class JoinService
    {
        private readonly object lockObject = new();
        // first key is server id, second key is user id
        private readonly Dictionary<ulong, Dictionary<ulong, UserGuildPlayStatus>> storage = new();

        public bool ProcessShouldPlay(ulong serverId, ulong userId)
        {
            lock (lockObject)
            {
                if (!storage.TryGetValue(serverId, out var userStore))
                {
                    userStore = new Dictionary<ulong, UserGuildPlayStatus>();
                    storage.Add(serverId, userStore);
                }
                if (!userStore.TryGetValue(userId, out var playStatus))
                {
                    playStatus = new UserGuildPlayStatus { LastLeft = DateTime.MinValue, CanPlay = true };
                    userStore.Add(userId, playStatus);
                }
                var returnValue = playStatus.CanPlay && DateTime.Now.AddMinutes(-2) > playStatus.LastLeft;
                playStatus.CanPlay = false;
                return returnValue;
            }
        }

        public void ProcessLeave(ulong serverId, ulong userId)
        {
            lock (lockObject)
            {
                if (!storage.TryGetValue(serverId, out var userStore))
                {
                    userStore = new Dictionary<ulong, UserGuildPlayStatus>();
                    storage.Add(serverId, userStore);
                }
                if (userStore.TryGetValue(userId, out var playStatus))
                {
                    playStatus.LastLeft = DateTime.Now;
                    playStatus.CanPlay = true;
                }
                else
                {
                    playStatus = new UserGuildPlayStatus { LastLeft = DateTime.Now, CanPlay = true };
                    userStore.Add(userId, playStatus);
                }
            }
        }
    }

    class UserGuildPlayStatus
    {
        public DateTime LastLeft { get; set; }
        public bool CanPlay { get; set; }
    }
}
