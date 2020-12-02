using System;
using System.Collections.Generic;

namespace Photizer.Domain.Entities
{
    public class Picture : BaseEntity
    {
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public int Rating { get; set; }
        public string FileType { get; set; }
        public string ShutterSpeed { get; set; }
        public string Aperture { get; set; }
        public string ISOSpeed { get; set; }
        public string FocalLength { get; set; }

        public Category Category { get; set; }
        public Camera Camera { get; set; }
        public Lense Lense { get; set; }
        public Location Location { get; set; }

        public List<PictureTag> PictureTags { get; set; }
        public List<PicturePerson> PicturePeople { get; set; }
        public List<CollectionPicture> CollectionPictures { get; set; }
    }
}