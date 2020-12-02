using Photizer.Domain.Enums;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Photizer.ImageUtilities
{
    public interface IBitmapImageResizer
    {
        Task<BitmapImage> GetResizedBitmapImage(string filePath, ImageSize size);
    }
}