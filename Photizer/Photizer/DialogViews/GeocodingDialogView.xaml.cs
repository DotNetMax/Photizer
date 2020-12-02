using Caliburn.Micro;
using MapControl;
using Photizer.Domain.EventMessages;
using System.Windows.Controls;
using System.Windows.Input;

namespace Photizer.DialogViews
{
    /// <summary>
    /// Interaction logic for GeocodingDialogView.xaml
    /// </summary>
    public partial class GeocodingDialogView : UserControl
    {
        public GeocodingDialogView()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "Photizer Map Control");
            var cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            TileImageLoader.Cache = cache;
            InitializeComponent();

            this.Map.MouseRightButtonDown += Map_MouseRightButtonDown; ;
        }

        private void Map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mapObj = (e.Source as Map);
            var center = mapObj.Center;

            var eventAggregator = IoC.Get<IEventAggregator>();
            eventAggregator.PublishOnCurrentThreadAsync(new PassCoordinatesMessage() { Latitude = center.Latitude, Longitude = center.Longitude });
        }
    }
}