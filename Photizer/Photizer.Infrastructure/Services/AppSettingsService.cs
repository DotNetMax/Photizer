using Newtonsoft.Json;
using Photizer.Domain.Interfaces;
using Photizer.Domain.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private AppSettings _appSettings;
        public FileInfo SettingsPath { get; private set; }

        public AppSettingsService()
        {
            _appSettings = new AppSettings();
            SettingsPath = GetSettingsFilePath();
        }

        public string GetSettingsDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData
                , Environment.SpecialFolderOption.DoNotVerify), "PhotizerData");
        }

        private static FileInfo GetSettingsFilePath()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData
                , Environment.SpecialFolderOption.DoNotVerify), "PhotizerData");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += @"\Photizer.json";
            return new FileInfo(path);
        }

        public DirectoryInfo GetBackupPicturesFolder()
        {
            return _appSettings.BackupPicturesFolder;
        }

        public DirectoryInfo GetMainPicturesFolder()
        {
            return _appSettings.MainPicturesFolder;
        }

        public async Task LoadSettingsAsync()
        {
            if (File.Exists(SettingsPath.FullName))
            {
                string json = await ReadTextAsync(SettingsPath.FullName).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(json))
                {
                    _appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (!string.IsNullOrEmpty(_appSettings.MainPicturesFolderPath))
                    {
                        _appSettings.MainPicturesFolder = new DirectoryInfo(_appSettings.MainPicturesFolderPath);
                    }
                    if (!string.IsNullOrEmpty(_appSettings.BackupPicturesFolderPath))
                    {
                        _appSettings.BackupPicturesFolder = new DirectoryInfo(_appSettings.BackupPicturesFolderPath);
                    }
                }
                else
                {
                    _appSettings = new AppSettings();
                }
            }
            else
            {
                _appSettings = new AppSettings();
            }
        }

        private async Task SaveSetingsAsync()
        {
            if (_appSettings != null)
            {
                AppSettings appSettingsToSave = new AppSettings
                {
                    MainPicturesFolderPath = _appSettings.MainPicturesFolderPath,
                    BackupPicturesFolderPath = _appSettings.BackupPicturesFolderPath
                };

                string json = JsonConvert.SerializeObject(appSettingsToSave);
                await WriteTextAsync(SettingsPath.FullName, json).ConfigureAwait(false);
            }
        }

        public async Task SetBackupPicturesFolderAsync(DirectoryInfo info)
        {
            if (_appSettings != null)
            {
                _appSettings.BackupPicturesFolder = info;
                _appSettings.BackupPicturesFolderPath = info.FullName;
                await SaveSetingsAsync().ConfigureAwait(false);
            }
        }

        public async Task SetMainPicturesFolderAsync(DirectoryInfo info)
        {
            if (_appSettings != null)
            {
                _appSettings.MainPicturesFolder = info;
                _appSettings.MainPicturesFolderPath = info.FullName;
                await SaveSetingsAsync().ConfigureAwait(false);
            }
        }

        private static async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length).ConfigureAwait(false);
            };
        }

        private static async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
                {
                    string text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }

        public AppSettings GetSettings()
        {
            return _appSettings;
        }
    }
}