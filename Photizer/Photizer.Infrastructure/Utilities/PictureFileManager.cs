using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Utilities
{
    public class PictureFileManager : IPictureFileManager
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly IPhotizerLogger _logger;

        public PictureFileManager(IAppSettingsService appSettingsService, IPhotizerLogger logger)
        {
            _appSettingsService = appSettingsService;
            _logger = logger;
        }

        public async Task<bool> AddPictureFileToManagedFoldersAsync(FileInfo source, Picture picture)
        {
            string destinationFileMain = _appSettingsService.GetMainPicturesFolder().FullName + $"{picture.Id}{picture.FileType}";
            string destinationFileBackup = _appSettingsService.GetBackupPicturesFolder().FullName + $"{picture.Id}{picture.FileType}";
            await CopyFileAsync(source, destinationFileMain).ConfigureAwait(false);
            await CopyFileAsync(source, destinationFileBackup).ConfigureAwait(false);

            if (File.Exists(destinationFileMain) && File.Exists(destinationFileBackup))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ExportPictureAsync(DirectoryInfo destinationFolder, Picture picture)
        {
            FileInfo sourceFile = new FileInfo(_appSettingsService.GetMainPicturesFolder().FullName + $"{picture.Id}{picture.FileType}");
            string destinationFile = destinationFolder.FullName + $"{picture.Id}{picture.FileType}";
            await CopyFileAsync(sourceFile, destinationFile).ConfigureAwait(false);
            if (File.Exists(destinationFile))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public FileInfo GetPictureFileByPicture(Picture picture)
        {
            try
            {
                FileInfo pictureFile = new FileInfo(_appSettingsService.GetMainPicturesFolder().FullName + $"{picture.Id}{picture.FileType}");
                if (File.Exists(pictureFile.FullName))
                {
                    return pictureFile;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool RemovePictureFileFromManagedFolders(Picture picture)
        {
            FileInfo mainFile = new FileInfo(_appSettingsService.GetMainPicturesFolder().FullName + $"{picture.Id}{picture.FileType}");
            FileInfo backupFile = new FileInfo(_appSettingsService.GetBackupPicturesFolder().FullName + $"{picture.Id}{picture.FileType}");

            string mainFilePath = mainFile.FullName;
            string backupFilePath = backupFile.FullName;

            mainFile.Delete();
            backupFile.Delete();

            if (!File.Exists(mainFilePath) && !File.Exists(backupFilePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task CopyFileAsync(FileInfo sourceFile, string destinationPath)
        {
            using (Stream source = File.OpenRead(sourceFile.FullName))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    try
                    {
                        await source.CopyToAsync(destination).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error moving Pictures: source-> {source} destination-> {destination}", ex, sourceFile.FullName, destinationPath);
                    }
                }
            }
        }
    }
}