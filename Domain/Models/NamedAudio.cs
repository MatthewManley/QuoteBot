namespace Domain.Models
{
    public class NamedAudio
    {
        public Audio Audio { get; set; }
        public AudioOwner AudioOwner { get; set; }
    }
}
