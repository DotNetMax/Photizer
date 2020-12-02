using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class DetailWasEditedMessage
    {
        public Picture Picture { get; set; }
    }
}