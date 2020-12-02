using System;
using System.Collections.Generic;

namespace Photizer.Domain.Entities
{
    public class Collection : BaseEntity
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public List<CollectionPicture> CollectionPictures { get; set; }
    }
}