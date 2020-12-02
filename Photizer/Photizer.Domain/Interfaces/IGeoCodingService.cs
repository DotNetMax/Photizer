using Photizer.Domain.Entities;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface IGeoCodingService
    {
        Task<Location> GetCoordinatesForLocation(Location location);

        Task<(string country, string place)> GetLocationByCoordinates(double latitude, double longitude);
    }
}