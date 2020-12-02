using Photizer.Domain.Entities;

namespace Photizer.Domain.EventMessages
{
    public class EditCategoryMessage
    {
        public Category EditedCategory { get; set; }
    }
}