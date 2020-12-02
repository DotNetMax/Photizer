using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Models;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddPictureSettingsViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        private bool _keepCategory;
        private bool _keepTags;
        private bool _keepPeople;
        private bool _keepLocation;

        public bool KeepLocation
        {
            get { return _keepLocation; }
            set
            {
                _keepLocation = value;
                NotifyOfPropertyChange();
            }
        }

        public bool KeepPeople
        {
            get { return _keepPeople; }
            set
            {
                _keepPeople = value;
                NotifyOfPropertyChange();
            }
        }

        public bool KeepTags
        {
            get { return _keepTags; }
            set
            {
                _keepTags = value;
                NotifyOfPropertyChange();
            }
        }

        public bool KeepCategory
        {
            get { return _keepCategory; }
            set
            {
                _keepCategory = value;
                NotifyOfPropertyChange();
            }
        }

        public AddPictureSettingsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        protected override void OnViewLoaded(object view)
        {
            _eventAggregator.SubscribeOnPublishedThread(this);
            base.OnViewLoaded(view);
        }

        public async Task SaveSettings()
        {
            AddPictureSettings settings = new AddPictureSettings
            {
                KeepCategory = KeepCategory,
                KeepPeople = KeepPeople,
                KeepTags = KeepTags,
                KeepLocation = KeepLocation
            };
            await _eventAggregator.PublishOnCurrentThreadAsync(new AddPictureSettingsMessage { AddPictureSettings = settings }).ConfigureAwait(false);
        }
    }
}