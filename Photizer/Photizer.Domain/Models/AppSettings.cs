using System.IO;

namespace Photizer.Domain.Models
{
    public class AppSettings
    {
        public DirectoryInfo MainPicturesFolder { get; set; }
        public DirectoryInfo BackupPicturesFolder { get; set; }
        public string MainPicturesFolderPath { get; set; }
        public string BackupPicturesFolderPath { get; set; }
    }
}