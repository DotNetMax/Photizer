using MetadataExtractor;
using Photizer.Domain.Enums;
using Photizer.Domain.Interfaces;
using Photizer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Photizer.Infrastructure.Utilities
{
    public class ExifDataExtractor : IExifDataExtractor
    {
        private readonly IPhotizerLogger _logger;

        public ExifDataExtractor(IPhotizerLogger logger)
        {
            _logger = logger;
        }

        public ExifData ExtractExifData(string filePath)
        {
            try
            {
                IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(filePath);

                List<MetadataExtractor.Tag> tags = new List<MetadataExtractor.Tag>();

                foreach (var d in directories)
                {
                    tags.AddRange(d.Tags);
                }

                ExifData data = new ExifData();

                if (tags.Any(t => t.Type == (int)ExifTagType.Camera))
                {
                    data.Camera = tags.Where(t => t.Type == (int)ExifTagType.Camera).Select(t => t.Description).FirstOrDefault();
                }

                if (tags.Any(t => t.Type == (int)ExifTagType.Lense))
                {
                    data.Lense = tags.Where(t => t.Type == (int)ExifTagType.Lense).Select(t => t.Description).FirstOrDefault();
                }

                if (tags.Any(t => t.Type == (int)ExifTagType.ShutterSpeed))
                {
                    data.ShutterSpeed = tags.Where(t => t.Type == (int)ExifTagType.ShutterSpeed).Select(t => t.Description).FirstOrDefault();
                }

                if (tags.Any(t => t.Type == (int)ExifTagType.Aperture))
                {
                    data.Aperture = tags.Where(t => t.Type == (int)ExifTagType.Aperture).Select(t => t.Description).FirstOrDefault();
                }

                if (tags.Any(t => t.Type == (int)ExifTagType.ISOSpeed))
                {
                    data.ISOSpeed = "ISO " + tags.Where(t => t.Type == (int)ExifTagType.ISOSpeed).Select(t => t.Description).FirstOrDefault();
                }

                if (tags.Any(t => t.Type == (int)ExifTagType.FocalLength))
                {
                    data.FocalLength = tags.Where(t => t.Type == (int)ExifTagType.FocalLength).Select(t => t.Description).FirstOrDefault();
                }

                if (tags.Any(t => t.Type == (int)ExifTagType.Created))
                {
                    data.Created = DateTime.ParseExact(tags.Where(t => t.Type == (int)ExifTagType.Created).Select(t => t.Description).FirstOrDefault(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error reading EXIF Data", ex);
                return null;
            }
        }
    }
}