using Photizer.Domain.Entities;
using System.Windows.Media.Imaging;

namespace Photizer.ImageUtilities
{
    public class ImageData
    {
        public string FilePath { get; set; }
        public bool IsSelected { get; set; }
        public BitmapImage Image { get; set; }
        public Picture Picture { get; set; }
    }
}