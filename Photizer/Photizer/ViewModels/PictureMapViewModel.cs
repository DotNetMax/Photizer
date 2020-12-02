using Caliburn.Micro;
using MapControl;
using Photizer.Domain.Enums;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.ImageUtilities;
using Photizer.Infrastructure.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Photizer.ViewModels
{
    public class PushpinItem : Screen
    {
        public MapControl.Location Location { get; set; }
        public Domain.Entities.Location LocationEntity { get; set; }
        public string Name { get; set; }
        public ObservableCollection<ImageData> Thumbnails { get; set; }

        private Visibility _thumbnailVisibility;

        public Visibility ThumbnailVisibility
        {
            get { return _thumbnailVisibility; }
            set
            {
                _thumbnailVisibility = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public class PictureMapViewModel : Screen
    {
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IBitmapImageResizer _bitmapImageResizer;
        private readonly IPictureFileManager _pictureFileManager;
        private readonly IEventAggregator _eventAggregator;

        private Location _mapCenter;

        public Location MapCenter
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

        private ObservableCollection<PushpinItem> _pins;

        public ObservableCollection<PushpinItem> Pins
        {
            get { return _pins; }
            set
            {
                _pins = value;
                NotifyOfPropertyChange();
            }
        }

        private int _mapZoomLevel;

        public int MapZoomLevel
        {
            get { return _mapZoomLevel; }
            set
            {
                _mapZoomLevel = value;
                if (value >= 13)
                {
                    ChangeThumbnailVisibility(true);
                }
                else
                {
                    ChangeThumbnailVisibility(false);
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _thumbnailsAreVisible;

        public PictureMapViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IBitmapImageResizer bitmapImageResizer, IPictureFileManager pictureFileManager, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _bitmapImageResizer = bitmapImageResizer;
            _pictureFileManager = pictureFileManager;
            _eventAggregator = eventAggregator;

            MapLayer = new MapTileLayer
            {
                SourceName = "OpenStreetMap",
                Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                TileSource = new TileSource { UriFormat = "https://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                MaxZoomLevel = 16
            };
        }

        private void ChangeThumbnailVisibility(bool show)
        {
            if (show && !_thumbnailsAreVisible && Pins != null)
            {
                foreach (var pin in Pins)
                {
                    pin.ThumbnailVisibility = Visibility.Visible;
                }
                _thumbnailsAreVisible = true;
            }
            else if (!show && _thumbnailsAreVisible)
            {
                foreach (var pin in Pins)
                {
                    pin.ThumbnailVisibility = Visibility.Collapsed;
                }
                _thumbnailsAreVisible = false;
            }
            else if (Pins == null)
            {
                _thumbnailsAreVisible = false;
            }
        }

        private async Task LoadMap()
        {
            var locations = await _photizerUnitOfWork.LocationRepository.GetAllWithPictures().ConfigureAwait(true);

            if (locations != null && locations.Any())
            {
                if (MapCenter == null)
                {
                    MapCenter = LocationEntityToMapControlConverter(locations.First());
                }

                List<PushpinItem> pinItems = new List<PushpinItem>();

                foreach (var location in locations)
                {
                    List<ImageData> imageData = new List<ImageData>();

                    if (location.Pictures != null)
                    {
                        foreach (var picture in location.Pictures.Take(3))
                        {
                            var image = await _bitmapImageResizer.GetResizedBitmapImage(_pictureFileManager.GetPictureFileByPicture(picture).FullName, ImageSize.Small).ConfigureAwait(true);
                            ImageData imgData = new ImageData
                            {
                                Image = image
                            };
                            System.Windows.Application.Current.Dispatcher.Invoke(delegate
                            {
                                imageData.Add(imgData);
                            });
                        }
                    }

                    pinItems.Add(new PushpinItem()
                    {
                        Location = LocationEntityToMapControlConverter(location),
                        Name = location.Place,
                        Thumbnails = new ObservableCollection<ImageData>(imageData),
                        ThumbnailVisibility = Visibility.Collapsed,
                        LocationEntity = location
                    });
                }

                Pins = new ObservableCollection<PushpinItem>(pinItems);
            }
        }

        private static Location LocationEntityToMapControlConverter(Domain.Entities.Location location)
        {
            Location converted = new Location(location.Latitude, location.Longitude);
            return converted;
        }

        public async Task OpenSearchWithLocation(Domain.Entities.Location location)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToPictureSearchMessage()).ConfigureAwait(true);
            await _eventAggregator.PublishOnCurrentThreadAsync(new DoSearchWithLocationMessage { Location = location }).ConfigureAwait(false);
        }

        protected override void OnViewLoaded(object view)
        {
            _thumbnailsAreVisible = false;
            MapZoomLevel = 11;

            Task.Run(LoadMap);
        }
    }
}