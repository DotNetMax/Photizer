using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Repositories
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly DbContext _ctx;
        private readonly IPhotizerLogger _logger;

        public CollectionRepository(DbContext context, IPhotizerLogger logger)
        {
            _ctx = context;
            _logger = logger;
        }

        public async Task<Collection> Add(Collection entity)
        {
            try
            {
                _ctx.Set<Collection>().Add(entity);
                return await Task.FromResult(entity).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error adding new collection -> {value}", exception, entity);
                return null;
            }
        }

        public async Task AddPicture(Collection collection, List<Picture> pictures)
        {
            var dbCollection = await _ctx.Set<Collection>()
                .Include(c => c.CollectionPictures).ThenInclude(cp => cp.Picture)
                .Where(c => c.Id == collection.Id).FirstOrDefaultAsync().ConfigureAwait(false);
            foreach (var picture in pictures)
            {
                if (!dbCollection.CollectionPictures.Any(cp => cp.PictureId == picture.Id))
                {
                    dbCollection.CollectionPictures.Add(new CollectionPicture { Collection = dbCollection, Picture = picture });
                }
            }
        }

        public async Task<bool> Delete(Collection entity)
        {
            _ctx.Set<Collection>().Remove(entity);
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Collection>> GetAll()
        {
            return await _ctx.Set<Collection>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<Collection>> GetAllWithPictures()
        {
            return await _ctx.Set<Collection>().Include(pic => pic.CollectionPictures).ThenInclude(cp => cp.Picture).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Collection> GetWithPictures(int id)
        {
            return await _ctx.Set<Collection>().Include(pic => pic.CollectionPictures)
                    .ThenInclude(cp => cp.Picture).Where(c => c.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
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