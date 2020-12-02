using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class DoSearchWithLocationMessage
    {
        public Location Location { get; set; }
    }
}