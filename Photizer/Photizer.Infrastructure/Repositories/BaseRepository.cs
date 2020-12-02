using Microsoft.EntityFrameworkCore;
using Photizer.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly DbContext _ctx;
        private readonly IPhotizerLogger _logger;

        public BaseRepository(DbContext context, IPhotizerLogger logger)
        {
            _ctx = context;
            _logger = logger;
        }

        public async Task<T> Add(T entity)
        {
            try
            {
                _ctx.Set<T>().Add(entity);
                return await Task.FromResult(entity);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error adding new entity of type {type} -> {value}", exception, nameof(T), entity);
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _ctx.Set<T>().ToListAsync();
        }

        public async Task<bool> Delete(T entity)
        {
            _ctx.Set<T>().Remove(entity);
            return await Task.FromResult(true);
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