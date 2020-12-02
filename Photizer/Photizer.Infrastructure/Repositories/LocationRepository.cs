using Microsoft.EntityFrameworkCore;
using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly DbContext _ctx;
        private readonly IPhotizerLogger _logger;

        public LocationRepository(DbContext context, IPhotizerLogger logger)
        {
            _ctx = context;
            _logger = logger;
        }

        public async Task<Location> Add(Location entity)
        {
            try
            {
                _ctx.Set<Location>().Add(entity);
                return await Task.FromResult(entity).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error adding new location -> {value}", exception, entity);
                return null;
            }
        }

        public async Task<bool> Delete(Location entity)
        {
            _ctx.Set<Location>().Remove(entity);
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Location>> GetAll()
        {
            return await _ctx.Set<Location>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<Location>> GetAllWithPictures()
        {
            return await _ctx.Set<Location>().Include(loc => loc.Pictures).ToListAsync().ConfigureAwait(false);
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