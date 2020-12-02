using Caliburn.Micro;
using MapControl;
using MaterialDesignThemes.Wpf;
using Photizer.Domain.Entities;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Photizer.DialogViewModels
{
    public class PinItem
    {
        public MapControl.Location PinLocation { get; set; }
        public string Name { get; set; }
    }

    public class GeocodingDialogViewModel : Screen, IHandle<PassLocationMessage>, IHandle<PassCoordinatesMessage>, IDisposable
    {
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IGeoCodingService _geoCodingService;
        private readonly IEventAggregator _eventAggregator;

        private SnackbarMessageQueue _messageQueue;

        public SnackbarMessageQueue MessageQueue
        {
            get { return _messageQueue; }
            set
            {
                _messageQueue = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<PinItem> _pins;

        public ObservableCollection<PinItem> Pins
        {
            get { return _pins; }
            set
            {
                _pins = value;
                NotifyOfPropertyChange();
            }
        }

        private MapControl.Location _mapCenter;

        public MapControl.Location MapCenter
        {
            get { return _mapCenter; }
            set
            {
                _mapCenter = value;
                NotifyOfPropertyChange();
            }
        }

        private UIElement _mapLayer;

        public UIElement MapLayer
        {
            get { return _mapLayer; }
            set
            {
                _mapLayer = value;
                NotifyOfPropertyChange();
            }
        }

        private Domain.Entities.Location _location;

        public Domain.Entities.Location Location
        {
            get { return _location; }
            set
            {
                _location = value;
                NotifyOfPropertyChange();
            }
        }

        public GeocodingDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IGeoCodingService geoCodingService, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _geoCodingService = geoCodingService;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
            MessageQueue = new SnackbarMessageQueue();
        }

        private static MapControl.Location LocationEntityToMapControlConverter(Domain.Entities.Location location)
        {
            MapControl.Location converted = new MapControl.Location(location.Latitude, location.Longitude);
            return converted;
        }

        private void LoadMap()
        {
            MapLayer = new MapTileLayer
            {
                SourceName = "OpenStreetMap",
                Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                TileSource = new TileSource { UriFormat = "https://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                MaxZoomLevel = 21
            };

            MapCenter = LocationEntityToMapControlConverter(Location);

            PinItem pinItem = new PinItem { PinLocation = LocationEntityToMapControlConverter(Location), Name = Location.Place };
            List<PinItem> pinItems = new List<PinItem>
            {
                pinItem
            };

            Pins = new ObservableCollection<PinItem>(pinItems);
        }

        private async Task UpdateCoordinates(bool update = false)
        {
            if (!update)
            {
                if (Location.Latitude == 0 || Location.Longitude == 0)
                {
                    Location = await _geoCodingService.GetCoordinatesForLocation(Location).ConfigureAwait(false);
                    if (Location.Latitude == 0 && Location.Longitude == 0)
                    {
                        MessageQueue.Enqueue(Multilang.GeocodingDialog_Error);
                    }
                }
            }
            else
            {
                Location = await _geoCodingService.GetCoordinatesForLocation(Location).ConfigureAwait(false);
                if (Location.Latitude == 0 && Location.Longitude == 0)
                {
                    MessageQueue.Enqueue(Multilang.GeocodingDialog_Error);
                }
            }
        }

        public async Task Geocode()
        {
            await UpdateCoordinates(true).ConfigureAwait(true);
            LoadMap();
        }

        public async Task SaveCoordinates()
        {
            await _photizerUnitOfWork.Save().ConfigureAwait(false);
            MessageQueue.Enqueue(Multilang.GeocodingDialog_UpdatedCoordinates);
        }

        public void UpdateCoords(ActionExecutionContext context)
        {
            if (context.EventArgs is KeyEventArgs keyArgs && keyArgs.Key == Key.Enter)
            {
                LoadMap();
            }
        }

        public async Task HandleAsync(PassLocationMessage message, CancellationToken cancellationToken)
        {
            Location = message.Location;
            await UpdateCoordinates().ConfigureAwait(true);
            LoadMap();
        }

        public Task HandleAsync(PassCoordinatesMessage message, CancellationToken cancellationToken)
        {
            Location.Latitude = message.Latitude;
            Location.Longitude = message.Longitude;
            NotifyOfPropertyChange(() => Location);

            LoadMap();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            MessageQueue?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}