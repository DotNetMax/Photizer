using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class EditTagMessage
    {
        public Tag EditedTag { get; set; }
    }
}