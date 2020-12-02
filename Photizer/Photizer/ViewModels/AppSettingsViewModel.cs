using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Photizer.ViewModels
{
    public class AppSettingsViewModel : Conductor<object>, IHandle<NavigateToFolderSettingsViewMessage>
    {
        private readonly IEventAggregator _eventAggregator;

        public AppSettingsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public async Task NavigateToFolderSettings()
        {
            await ActivateItemAsync(IoC.Get<FolderSettingsViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToDatabaseImportExport()
        {
            await ActivateItemAsync(IoC.Get<DatabaseImportExportViewModel>()).ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateToFolderSettingsViewMessage message, CancellationToken cancellationToken)
        {
            await NavigateToFolderSettings().ConfigureAwait(false);
        }
    }
}