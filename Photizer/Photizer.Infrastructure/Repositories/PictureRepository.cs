using Microsoft.EntityFrameworkCore;
using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Repositories
{
    public class PictureRepository : IPictureRepository
    {
        private readonly DbContext _ctx;
        private readonly IPhotizerLogger _logger;

        public PictureRepository(DbContext context, IPhotizerLogger logger)
        {
            _ctx = context;
            _logger = logger;
        }

        public async Task<Picture> Add(Picture entity)
        {
            try
            {
                _ctx.Set<Picture>().Add(entity);
                return await Task.FromResult(entity).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error adding new picture -> {value}", exception, entity);
                return null;
            }
        }

        public async Task<IEnumerable<Picture>> GetAll()
        {
            return await _ctx.Set<Picture>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<bool> Delete(Picture entity)
        {
            _ctx.Set<Picture>().Remove(entity);
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public async Task<List<Picture>> GetAllByCategory(Category category)
        {
            return await _ctx.Set<Picture>().Include(pic => pic.Category).Where(pic => pic.Category == category).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<Picture>> GetAllByCamera(Camera camera)
        {
            return await _ctx.Set<Picture>().Include(pic => pic.Camera).Where(pic => pic.Camera == camera).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<Picture>> GetAllByLense(Lense lense)
        {
            return await _ctx.Set<Picture>().Include(pic => pic.Lense).Where(pic => pic.Lense == lense).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<Picture>> GetAllByLocation(Location location)
        {
            return await _ctx.Set<Picture>().Include(pic => pic.Location).Where(pic => pic.Location == location).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<Picture>> GetAllByPeople(List<Person> people)
        {
            try
            {
                List<Picture> result = new List<Picture>();
                List<int> pictureIds = new List<int>();

                foreach (var person in people)
                {
                    var resultIds = await _ctx.Set<PicturePerson>().Where(pp => pp.PersonId == person.Id).Select(pp => pp.PictureId).ToListAsync().ConfigureAwait(false);
                    pictureIds.AddRange(resultIds);
                }
                var group = pictureIds.GroupBy(p => p).Select(p => new { p.Key, Count = p.Count() });
                var duplicates = group.Where(g => g.Count >= people.Count).Select(g => g.Key).ToList();
                var pictures = await _ctx.Set<Picture>().Where(p => duplicates.Contains(p.Id)).ToListAsync().ConfigureAwait(false);

                result.AddRange(pictures);
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error loading pictures for people", exception);
                return null;
            }
        }

        public async Task<List<Picture>> GetAllByTags(List<Tag> tags)
        {
            try
            {
                List<Picture> result = new List<Picture>();
                List<int> pictureIds = new List<int>();
                foreach (var tag in tags)
                {
                    var resultIds = await _ctx.Set<PictureTag>().Where(pt => pt.TagId == tag.Id).Select(pt => pt.PictureId).ToListAsync().ConfigureAwait(false);
                    pictureIds.AddRange(resultIds);
                }
                var group = pictureIds.GroupBy(p => p).Select(p => new { p.Key, Count = p.Count() });
                var duplicates = group.Where(g => g.Count >= tags.Count).Select(g => g.Key).ToList();
                var pictures = await _ctx.Set<Picture>().Where(p => duplicates.Contains(p.Id)).ToListAsync().ConfigureAwait(false);

                result.AddRange(pictures);
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error loading pictures for tags", exception);
                return null;
            }
        }

        public async Task<DateTime> GetEarliestCreatedDate()
        {
            var earliestPicture = await _ctx.Set<Picture>().OrderBy(pic => pic.Created).FirstOrDefaultAsync().ConfigureAwait(false);
            if (earliestPicture != null)
            {
                return earliestPicture.Created;
            }
            else
            {
                return new DateTime(DateTime.Now.Year, 1, 1);
            }
        }

        public async Task<DateTime> GetLatestCreatedDate()
        {
            var latestPicture = await _ctx.Set<Picture>().OrderByDescending(pic => pic.Created).FirstOrDefaultAsync().ConfigureAwait(false);
            if (latestPicture != null)
            {
                return latestPicture.Created;
            }
            else
            {
                return DateTime.Now.Date;
            }
        }

        public async Task<List<Picture>> GetByRating(int rating, string ratingParameter)
        {
            try
            {
                List<Picture> result = new List<Picture>();
                if (ratingParameter.Equals("="))
                {
                    result = await _ctx.Set<Picture>().Where(pic => pic.Rating == rating).ToListAsync().ConfigureAwait(false);
                }
                else if (ratingParameter.Equals(">"))
                {
                    result = await _ctx.Set<Picture>().Where(pic => pic.Rating > rating).ToListAsync().ConfigureAwait(false);
                }
                else if (ratingParameter.Equals("<"))
                {
                    result = await _ctx.Set<Picture>().Where(pic => pic.Rating < rating).ToListAsync().ConfigureAwait(false);
                }

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error loading pictures for rating", exception);
                return null;
            }
        }

        public async Task<List<Picture>> GetInDateTimeRange(DateTime from, DateTime to)
        {
            try
            {
                var dateResult = await _ctx.Set<Picture>().Where(pic => pic.Created.Date >= from.Date && pic.Created.Date <= to.Date).ToListAsync().ConfigureAwait(false);
                var timeResult = dateResult.Where(pic => pic.Created.TimeOfDay >= from.TimeOfDay && pic.Created.TimeOfDay <= to.TimeOfDay).ToList();
                return timeResult;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error loading pictures for date time range", exception);
                return null;
            }
        }

        public async Task<List<Picture>> GetByTitle(string title)
        {
            var result = await _ctx.Set<Picture>().Where(pic => pic.Title.ToLower().Contains(title.ToLower())).ToListAsync().ConfigureAwait(false);
            return result;
        }

        public async Task AddPerson(Picture picture, Person person)
        {
            var dbPicture = await _ctx.Set<Picture>().Include(pic => pic.PicturePeople).FirstOrDefaultAsync(pic => pic == picture).ConfigureAwait(false);
            if (dbPicture.PicturePeople == null)
            {
                dbPicture.PicturePeople = new List<PicturePerson>();
            }
            dbPicture.PicturePeople.Add(new PicturePerson() { Picture = dbPicture, Person = person });
        }

        public async Task AddTag(Picture picture, Tag tag)
        {
            var dbPicture = await _ctx.Set<Picture>().Include(pic => pic.PictureTags).FirstOrDefaultAsync(pic => pic == picture).ConfigureAwait(false);
            if (dbPicture.PictureTags == null)
            {
                dbPicture.PictureTags = new List<PictureTag>();
            }
            dbPicture.PictureTags.Add(new PictureTag() { Picture = dbPicture, Tag = tag });
        }

        public async Task<Picture> GetPictureWithAllDataById(int pictureId)
        {
            var picture = await _ctx.Set<Picture>()
                .Include(pic => pic.Category)
                .Include(pic => pic.Camera)
                .Include(pic => pic.Lense)
                .Include(pic => pic.Location)
                .Include(pic => pic.PictureTags).ThenInclude(pt => pt.Tag)
                .Include(pic => pic.PicturePeople).ThenInclude(pp => pp.Person)
                .Where(pic => pic.Id == pictureId).FirstOrDefaultAsync().ConfigureAwait(false);
            return picture;
        }

        public async Task RemovePerson(Picture picture, Person person)
        {
            var dbPicture = await _ctx.Set<Picture>().Include(pic => pic.PicturePeople).FirstOrDefaultAsync(pic => pic == picture).ConfigureAwait(false);
            if (dbPicture.PicturePeople == null)
            {
                dbPicture.PicturePeople = new List<PicturePerson>();
                return;
            }
            dbPicture.PicturePeople.Remove(dbPicture.PicturePeople.Where(pp => pp.Person == person).FirstOrDefault());
        }

        public async Task RemoveTag(Picture picture, Tag tag)
        {
            var dbPicture = await _ctx.Set<Picture>().Include(pic => pic.PictureTags).FirstOrDefaultAsync(pic => pic == picture).ConfigureAwait(false);
            if (dbPicture.PictureTags == null)
            {
                dbPicture.PictureTags = new List<PictureTag>();
                return;
            }
            dbPicture.PictureTags.Remove(dbPicture.PictureTags.Where(pt => pt.Tag == tag).FirstOrDefault());
        }

        public async Task<Picture> Reload(Picture picture)
        {
            await _ctx.Entry(picture).ReloadAsync().ConfigureAwait(false);
            return picture;
        }

        public async Task AddToCollection(Picture picture, Collection collection)
        {
            var dbPicture = await _ctx.Set<Picture>().Include(pic => pic.CollectionPictures).FirstOrDefaultAsync(pic => pic == picture).ConfigureAwait(false);
            if (dbPicture.CollectionPictures == null)
            {
                dbPicture.CollectionPictures = new List<CollectionPicture>();
            }

            dbPicture.CollectionPictures.Add(new CollectionPicture() { Picture = dbPicture, Collection = collection });
        }

        public async Task ReloadAll()
        {
            var all = await GetAll().ConfigureAwait(false);
            foreach (var entity in all)
            {
                await _ctx.Entry(entity).ReloadAsync().ConfigureAwait(false);
            }
        }
    }
}