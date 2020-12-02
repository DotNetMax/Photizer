using Photizer.Domain.Models;

namespace Photizer.Domain.Interfaces
{
    public interface IExifDataExtractor
    {
        ExifData ExtractExifData(string filePath);
    }
}