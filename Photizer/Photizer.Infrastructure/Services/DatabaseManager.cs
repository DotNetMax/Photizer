using Photizer.Domain.Interfaces;
using System;
using System.IO;

namespace Photizer.Infrastructure.Services
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly IAppSettingsService _appSettingsService;

        public DatabaseManager(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
        }

        public string GetDatabaseBackupFolderPath()
        {
            var directory = Path.Combine(_appSettingsService.GetSettingsDirectory(), "DatabaseBackups");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }

        public string ExportDatabase()
        {
            try
            {
                string source = Path.Combine(_appSettingsService.GetSettingsDirectory(), "Photizer.db");
                string destination = Path.Combine(GetDatabaseBackupFolderPath(), $"Photizer_{DateTime.Now.Ticks}.db");
                File.Copy(source, destination);
                return "Export finished";
            }
            catch (Exception ex)
            {
                return "Export failed: " + ex.Message;
            }
        }
    }
}