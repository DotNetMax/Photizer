using System;

namespace Photizer.Domain.Models
{
    public class ExifData
    {
        public string Camera { get; set; }
        public string Lense { get; set; }
        public string ShutterSpeed { get; set; }
        public string Aperture { get; set; }
        public string ISOSpeed { get; set; }
        public string FocalLength { get; set; }
        public DateTime Created { get; set; }
    }
}