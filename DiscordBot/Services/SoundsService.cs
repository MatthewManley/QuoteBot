using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class SoundsService
    {
        private Dictionary<string, string[]> soundBytes;

        public void UpdateSounds()
        {
            soundBytes = new Dictionary<string, string[]>();
            var dirs = Directory.GetDirectories("audio");
            foreach (var dir in dirs)
            {
                var files = Directory.GetFiles(dir, "*.mp3");
                if (files.Length > 0)
                    soundBytes.Add(dir.Substring(dir.IndexOf("\\") + 1), files.ToArray());
            }
        }

        public Dictionary<string, string[]> GetSounds()
        {
            if (soundBytes == null)
                UpdateSounds();
            return soundBytes;
        }
    }
}
