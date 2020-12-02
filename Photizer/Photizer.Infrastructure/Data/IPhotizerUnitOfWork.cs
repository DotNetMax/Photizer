using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Data
{
    public interface IPhotizerUnitOfWork
    {
        PhotizerDbContext PhotizerDbContext { get; }

        Task<bool> Save();

        IBaseRepository<Tag> TagRepository { get; }
        IBaseRepository<Person> PersonRepository { get; }
        IBaseRepository<Category> CategoryRepository { get; }
        IBaseRepository<Camera> CameraRepository { get; }
        IBaseRepository<Lense> LenseRepository { get; }
        ILocationRepository LocationRepository { get; }
        ICollectionRepository CollectionRepository { get; }
        IPictureRepository PictureRepository { get; }
    }
}