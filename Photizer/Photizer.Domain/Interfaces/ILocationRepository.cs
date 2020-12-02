using Photizer.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface ILocationRepository : IBaseRepository<Location>
    {
        Task<IEnumerable<Location>> GetAllWithPictures();
    }
}