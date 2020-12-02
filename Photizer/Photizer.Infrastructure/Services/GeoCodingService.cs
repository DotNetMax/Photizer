using Geocoding.Microsoft;
using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Services
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly IPhotizerLogger _logger;

        public GeoCodingService(IPhotizerLogger logger)
        {
            _logger = logger;
        }

        public async Task<Location> GetCoordinatesForLocation(Location location)
        {
            try
            {
                var geocoder = new BingMapsGeocoder("yMj7MhFnlG0dwLrSho73~jt1IY7lfwOUEbf7C-PNvow~AqBB1hJX__Z044KdvJuNNa_12_7kKmhHAzVe-0FkygxYk2iPR_z2Wr4sScPH8jXl");

                var addresses = await geocoder.GeocodeAsync($"{location.Country}, {location.Place}").ConfigureAwait(false);

                if (addresses != null)
                {
                    location.Longitude = addresses.First().Coordinates.Longitude;
                    location.Latitude = addresses.First().Coordinates.Latitude;
                }

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Geocoding Location {location}", ex, location);
                location.Latitude = 0;
                location.Longitude = 0;
                return location;
            }
        }

        public Task<(string country, string place)> GetLocationByCoordinates(double latitude, double longitude)
        {
            throw new NotImplementedException();
        }
    }
}