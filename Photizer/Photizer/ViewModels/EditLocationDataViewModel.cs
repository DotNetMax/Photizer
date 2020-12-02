using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Photizer.Commands;
using Photizer.DialogViewModels;
using Photizer.DialogViews;
using Photizer.Domain.Entities;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Photizer.ViewModels
{
    public class EditLocationDataViewModel : Screen
    {
        public ICommand OpenAddNewLocationDialogCommand => new DialogCommand(RunAddNewLocationDialog);

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPhotizerLogger _logger;

        private ObservableCollection<Location> _locations;

        public ObservableCollection<Location> Locations
        {
            get { return _locations; }
            set
            {
                _locations = value;
                NotifyOfPropertyChange();
            }
        }

        private Location _selectedLocation;

        public Location SelectedLocation
        {
            get { return _selectedLocation; }
            set
            {
                _selectedLocation = value;
                if (value is null)
                {
                    CanDeleteLocation = false;
                }
                else
                {
                    CanDeleteLocation = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _canDeleteLocation;

        public bool CanDeleteLocation
        {
            get { return _canDeleteLocation; }
            set
            {
                _canDeleteLocation = value;
                NotifyOfPropertyChange();
            }
        }

        public EditLocationDataViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IEventAggregator eventAggregator, IPhotizerLogger logger)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
            _logger = logger;
        }

        protected override void OnViewLoaded(object view)
        {
            Task.Run(() => LoadLocations());

            base.OnViewLoaded(view);
        }

        public async Task RefreshLocations()
        {
            await LoadLocations().ConfigureAwait(false);
        }

        public async Task DeleteLocation()
        {
            var askForDeleteResult = MessageBox.Show("Really delete this Location? " + SelectedLocation.Place, "Warning", MessageBoxButton.YesNo);
            if (askForDeleteResult == MessageBoxResult.Yes)
            {
                await _photizerUnitOfWork.LocationRepository.Delete(SelectedLocation).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshLocations().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChangesCountry(KeyEventArgs args, Location location, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                location.Country = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshLocations().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChangesPlace(KeyEventArgs args, Location location, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                location.Place = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshLocations().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChangesLatitude(KeyEventArgs args, Location location, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                try
                {
                    location.Latitude = Convert.ToDouble(source.Text);
                    await _photizerUnitOfWork.Save().ConfigureAwait(false);
                    await RefreshLocations().ConfigureAwait(false);
                    await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error converting " + source.Text + " into latitude");
                }
            }
        }

        public async Task SaveChangesLongitude(KeyEventArgs args, Location location, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                try
                {
                    location.Longitude = Convert.ToDouble(source.Text);
                    await _photizerUnitOfWork.Save().ConfigureAwait(false);
                    await RefreshLocations().ConfigureAwait(false);
                    await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error converting " + source.Text + " into longitude");
                }
            }
        }

        public async Task OpenGeocodingDialog(Location location)
        {
            try
            {
                var view = new GeocodingDialogView
                {
                    DataContext = IoC.Get<GeocodingDialogViewModel>()
                };
                await _eventAggregator.PublishOnUIThreadAsync(new PassLocationMessage { Location = location }).ConfigureAwait(true);
                await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
                await RefreshLocations().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error opening Geocoding Dialog", ex);
            }
        }

        private async Task RunAddNewLocationDialog()
        {
            var view = new AddNewLocationDialogView
            {
                DataContext = IoC.Get<AddNewLocationDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await RefreshLocations().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
        }

        private async Task LoadLocations()
        {
            var locations = await _photizerUnitOfWork.LocationRepository.GetAll().ConfigureAwait(false);
            locations = locations.OrderBy(l => l.Place).ToList();
            Locations = new ObservableCollection<Location>(locations);
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}