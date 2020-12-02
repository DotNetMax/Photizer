using Photizer.Domain.Models;
using System.IO;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface IAppSettingsService
    {
        FileInfo SettingsPath { get; }

        string GetSettingsDirectory();

        Task LoadSettingsAsync();

        Task SetMainPicturesFolderAsync(DirectoryInfo info);

        Task SetBackupPicturesFolderAsync(DirectoryInfo info);

        DirectoryInfo GetMainPicturesFolder();

        DirectoryInfo GetBackupPicturesFolder();

        AppSettings GetSettings();
    }
}