using Caliburn.Micro;
using Photizer.Domain.Interfaces;
using System.Diagnostics;
using System.Windows;

namespace Photizer.ViewModels
{
    public class DatabaseImportExportViewModel : Screen
    {
        private readonly IDatabaseManager _databaseManager;
        private readonly IAppSettingsService _appSettingsService;

        public DatabaseImportExportViewModel(IDatabaseManager databaseManager, IAppSettingsService appSettingsService)
        {
            _databaseManager = databaseManager;
            _appSettingsService = appSettingsService;
        }

        public void ImportDatabaseFile()
        {
            string backupDirectory = _databaseManager.GetDatabaseBackupFolderPath();
            string settingsDirectory = _appSettingsService.GetSettingsDirectory();

            Process.Start("explorer.exe", backupDirectory);
            Process.Start("explorer.exe", settingsDirectory);
        }

        public void ExportDatabaseFile()
        {
            string message = _databaseManager.ExportDatabase();
            MessageBox.Show(message);
        }
    }
}