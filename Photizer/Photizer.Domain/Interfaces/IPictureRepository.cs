using Photizer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photizer.Domain.Interfaces
{
    public interface IPictureRepository : IBaseRepository<Picture>
    {
        Task<List<Picture>> GetAllByCategory(Category category);

        Task<List<Picture>> GetAllByCamera(Camera camera);

        Task<List<Picture>> GetAllByLense(Lense lense);

        Task<List<Picture>> GetAllByLocation(Location location);

        Task<List<Picture>> GetAllByPeople(List<Person> people);

        Task<List<Picture>> GetAllByTags(List<Tag> tags);

        Task<Picture> GetPictureWithAllDataById(int pictureId);

        Task<DateTime> GetEarliestCreatedDate();

        Task<DateTime> GetLatestCreatedDate();

        Task<List<Picture>> GetByRating(int rating, string ratingParameter);

        Task<List<Picture>> GetInDateTimeRange(DateTime from, DateTime to);

        Task<List<Picture>> GetByTitle(string title);

        Task<Picture> Reload(Picture picture);

        Task AddPerson(Picture picture, Person person);

        Task AddTag(Picture picture, Tag tag);

        Task AddToCollection(Picture picture, Collection collection);

        Task RemovePerson(Picture picture, Person person);

        Task RemoveTag(Picture picture, Tag tag);
    }
}