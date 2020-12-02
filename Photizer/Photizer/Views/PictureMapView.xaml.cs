using MapControl;
using System.Windows.Controls;

namespace Photizer.Views
{
    /// <summary>
    /// Interaction logic for PictureMapView.xaml
    /// </summary>
    public partial class PictureMapView : UserControl
    {
        public PictureMapView()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "Photizer Map Control");
            var cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
            TileImageLoader.Cache = cache;

            InitializeComponent();
        }
    }
}