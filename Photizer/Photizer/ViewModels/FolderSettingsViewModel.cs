using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.Domain.Models;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Photizer.ViewModels
{
    public class FolderSettingsViewModel : Caliburn.Micro.Screen
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly IEventAggregator _eventAggregator;
        private AppSettings _appSettings;

        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set
            {
                _appSettings = value;
                NotifyOfPropertyChange();
            }
        }

        public FolderSettingsViewModel(IAppSettingsService appSettingsService, IEventAggregator eventAggregator)
        {
            _appSettingsService = appSettingsService;
            _eventAggregator = eventAggregator;
        }

        protected override void OnViewLoaded(object view)
        {
            AppSettings = _appSettingsService.GetSettings();
        }

        private async Task UpdateNavigationVisibility()
        {
            if (AppSettings.MainPicturesFolder != null && AppSettings.BackupPicturesFolder != null)
            {
                await _eventAggregator.PublishOnCurrentThreadAsync(new UpdateNavigationVisibilityMessage() { IsVisible = true }).ConfigureAwait(false);
            }
            else
            {
                await _eventAggregator.PublishOnCurrentThreadAsync(new UpdateNavigationVisibilityMessage() { IsVisible = false }).ConfigureAwait(false);
            }
        }

        public async Task SelectMainFolder()
        {
            using var folderBrowser = new FolderBrowserDialog();
            var result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                await _appSettingsService.SetMainPicturesFolderAsync(new DirectoryInfo(folderBrowser.SelectedPath + @"\")).ConfigureAwait(false);
                AppSettings = _appSettingsService.GetSettings();
                await UpdateNavigationVisibility().ConfigureAwait(false);
            }
        }

        public async Task SelectBackupFolder()
        {
            using var folderBrowser = new FolderBrowserDialog();
            var result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                await _appSettingsService.SetBackupPicturesFolderAsync(new DirectoryInfo(folderBrowser.SelectedPath + @"\")).ConfigureAwait(false);
                AppSettings = _appSettingsService.GetSettings();
                await UpdateNavigationVisibility().ConfigureAwait(false);
            }
        }
    }
}