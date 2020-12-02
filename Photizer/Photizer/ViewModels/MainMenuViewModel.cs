using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photizer.ViewModels
{
    public class MainMenuViewModel : Conductor<object>, IHandle<NavigateToDetailPageMessage>, IHandle<NavigateBackToPictureResultViewMessage>
        , IHandle<NavigateToCollectionsViewMessage>, IHandle<NavigateToPictureSearchMessage>, IHandle<UpdateNavigationVisibilityMessage>
    {
        private readonly IAppSettingsService _appSettingsService;
        private readonly IEventAggregator _eventAggregator;

        private string _currentVersion;

        public string CurrentVersion
        {
            get { return _currentVersion; }
            set
            {
                _currentVersion = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isNavigationEnabled;

        public bool IsNavigationEnabled
        {
            get { return _isNavigationEnabled; }
            set
            {
                _isNavigationEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public MainMenuViewModel(IAppSettingsService appSettingsService, IEventAggregator eventAggregator)
        {
            _appSettingsService = appSettingsService;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);

            CurrentVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        protected async override void OnViewLoaded(object view)
        {
            await _appSettingsService.LoadSettingsAsync().ConfigureAwait(false);

            if (_appSettingsService.GetMainPicturesFolder() == null)
            {
                IsNavigationEnabled = false;
                await ActivateItemAsync(IoC.Get<AppSettingsViewModel>()).ConfigureAwait(true);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToFolderSettingsViewMessage()).ConfigureAwait(true);
            }
            else
            {
                await ActivateItemAsync(IoC.Get<PictureSearchViewModel>()).ConfigureAwait(false);
                IsNavigationEnabled = true;
            }
        }

        public async Task NavigateToPicturesAsync()
        {
            await ActivateItemAsync(IoC.Get<PictureSearchViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToCollectionsAsync()
        {
            await ActivateItemAsync(IoC.Get<CollectionsViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToAddPicturesAsync()
        {
            await ActivateItemAsync(IoC.Get<AddPictureViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToEditDataAsync()
        {
            await ActivateItemAsync(IoC.Get<EditDataViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToMapAsync()
        {
            await ActivateItemAsync(IoC.Get<PictureMapViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToSettingsAsync()
        {
            await ActivateItemAsync(IoC.Get<AppSettingsViewModel>()).ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateToDetailPageMessage message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(IoC.Get<PictureDetailViewModel>(), cancellationToken).ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateBackToPictureResultViewMessage message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(IoC.Get<PictureSearchViewModel>(), cancellationToken).ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateToCollectionsViewMessage message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(IoC.Get<CollectionsViewModel>(), cancellationToken).ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateToPictureSearchMessage message, CancellationToken cancellationToken)
        {
            await ActivateItemAsync(IoC.Get<PictureSearchViewModel>(), cancellationToken).ConfigureAwait(false);
        }

        public Task HandleAsync(UpdateNavigationVisibilityMessage message, CancellationToken cancellationToken)
        {
            IsNavigationEnabled = message.IsVisible;
            return Task.CompletedTask;
        }
    }
}