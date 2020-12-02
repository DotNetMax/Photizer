using Photizer.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface ICollectionRepository : IBaseRepository<Collection>
    {
        Task<IEnumerable<Collection>> GetAllWithPictures();

        Task<Collection> GetWithPictures(int id);
    }
}