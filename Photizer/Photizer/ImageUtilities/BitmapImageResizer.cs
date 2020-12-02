using Photizer.Domain.Enums;
using Photizer.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Photizer.ImageUtilities
{
    public class BitmapImageResizer : IBitmapImageResizer
    {
        private readonly IPhotizerLogger _logger;

        public BitmapImageResizer(IPhotizerLogger logger)
        {
            _logger = logger;
        }

        public Task<BitmapImage> GetResizedBitmapImage(string filePath, ImageSize size)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(filePath);
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                switch (size)
                {
                    case ImageSize.Original:
                        break;

                    case ImageSize.Big:
                        image.DecodePixelWidth = (int)ImageSize.Big;
                        break;

                    case ImageSize.Medium:
                        image.DecodePixelWidth = (int)ImageSize.Medium;
                        break;

                    case ImageSize.Small:
                        image.DecodePixelWidth = (int)ImageSize.Small;
                        break;

                    default:
                        break;
                }

                image.EndInit();
                image.Freeze();

                return Task.FromResult(image);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating BitmapImage for file {file}.", ex, filePath);
                return null;
            }
        }
    }
}