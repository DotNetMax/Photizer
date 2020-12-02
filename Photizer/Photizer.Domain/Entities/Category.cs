using System.Collections.Generic;

namespace Photizer.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public List<Picture> Pictures { get; set; }
    }
}