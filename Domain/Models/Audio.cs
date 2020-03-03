namespace Domain.Models
{
    public class Audio
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Audio audio))
            {
                return false;
            }
            return Path.Equals(audio.Path);
        }
    }
}