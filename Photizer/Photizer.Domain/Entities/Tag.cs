using System.Collections.Generic;

namespace Photizer.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public List<PictureTag> PictureTags { get; set; }
    }
}