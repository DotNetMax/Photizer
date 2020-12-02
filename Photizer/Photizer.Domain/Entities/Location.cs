using System.Collections.Generic;

namespace Photizer.Domain.Entities
{
    public class Location : BaseEntity
    {
        public string Country { get; set; }
        public string Place { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public List<Picture> Pictures { get; set; }
    }
}