using Microsoft.EntityFrameworkCore;
using Photizer.Domain.Entities;
using Photizer.Domain.Interfaces;
using Photizer.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace Photizer.Infrastructure.Data
{
    public class PhotizerUnitOfWork : IPhotizerUnitOfWork
    {
        public PhotizerDbContext PhotizerDbContext { get; private set; }

        private IBaseRepository<Tag> _tagRepository;
        private IBaseRepository<Person> _personRepository;
        private IBaseRepository<Category> _categoryRepository;
        private IBaseRepository<Camera> _cameraRepository;
        private IBaseRepository<Lense> _lenseRepository;
        private ILocationRepository _locationRepository;
        private ICollectionRepository _collectionRepository;
        private IPictureRepository _pictureRepository;

        private readonly IPhotizerLogger _logger;

        public PhotizerUnitOfWork(PhotizerDbContext photizerDbContext, IPhotizerLogger logger)
        {
            PhotizerDbContext = photizerDbContext;

            PhotizerDbContext.Database.Migrate();

            _logger = logger;
        }

        public async Task<bool> Save()
        {
            try
            {
                await PhotizerDbContext.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError("Error saving changes to the database", exception);
                return false;
            }
        }

        public IBaseRepository<Person> PersonRepository
        {
            get
            {
                if (_personRepository == null)
                {
                    _personRepository = new BaseRepository<Person>(PhotizerDbContext, _logger);
                }
                return _personRepository;
            }
        }

        public IBaseRepository<Category> CategoryRepository
        {
            get
            {
                if (_categoryRepository == null)
                {
                    _categoryRepository = new BaseRepository<Category>(PhotizerDbContext, _logger);
                }
                return _categoryRepository;
            }
        }

        public IBaseRepository<Tag> TagRepository
        {
            get
            {
                if (_tagRepository == null)
                {
                    _tagRepository = new BaseRepository<Tag>(PhotizerDbContext, _logger);
                }
                return _tagRepository;
            }
        }

        public IBaseRepository<Camera> CameraRepository
        {
            get
            {
                if (_cameraRepository == null)
                {
                    _cameraRepository = new BaseRepository<Camera>(PhotizerDbContext, _logger);
                }
                return _cameraRepository;
            }
        }

        public IBaseRepository<Lense> LenseRepository
        {
            get
            {
                if (_lenseRepository == null)
                {
                    _lenseRepository = new BaseRepository<Lense>(PhotizerDbContext, _logger);
                }
                return _lenseRepository;
            }
        }

        public ILocationRepository LocationRepository
        {
            get
            {
                if (_locationRepository == null)
                {
                    _locationRepository = new LocationRepository(PhotizerDbContext, _logger);
                }
                return _locationRepository;
            }
        }

        public ICollectionRepository CollectionRepository
        {
            get
            {
                if (_collectionRepository == null)
                {
                    _collectionRepository = new CollectionRepository(PhotizerDbContext, _logger);
                }
                return _collectionRepository;
            }
        }

        public IPictureRepository PictureRepository
        {
            get
            {
                if (_pictureRepository == null)
                {
                    _pictureRepository = new PictureRepository(PhotizerDbContext, _logger);
                }
                return _pictureRepository;
            }
        }
    }
}