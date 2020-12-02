using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class EditPersonMessage
    {
        public Person EditedPerson { get; set; }
    }
}