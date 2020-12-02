using Photizer.Domain.Entities;
using System.IO;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface IPictureFileManager
    {
        Task<bool> AddPictureFileToManagedFoldersAsync(FileInfo source, Picture picture);

        bool RemovePictureFileFromManagedFolders(Picture picture);

        FileInfo GetPictureFileByPicture(Picture picture);

        Task<bool> ExportPictureAsync(DirectoryInfo destinationFolder, Picture picture);
    }
}