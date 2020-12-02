using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class EditLocationMessage
    {
        public Location EditedLocation { get; set; }
    }
}