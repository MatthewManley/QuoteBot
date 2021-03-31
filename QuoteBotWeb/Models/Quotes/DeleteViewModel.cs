using Domain.Models;

namespace QuoteBotWeb.Models.Quotes
{
    public class DeleteViewModel
    {
        public DeleteViewModel(NamedAudio namedAudio, ulong server)
        {
            NamedAudio = namedAudio;
            Server = server;
        }

        public NamedAudio NamedAudio { get; }
        public ulong Server { get; }
    }
}
