using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photizer.ViewModels
{
    public class SearchParameterViewModel : Screen, IHandle<RemoveTagMessage>, IHandle<RemovePersonMessage>, IHandle<NewPictureWasAddedMessage>
        , IHandle<DoSearchWithLocationMessage>, IDisposable, IHandle<ReloadPictureSearchAfterEditMessage>, IHandle<NotifyKeywordDataChangedMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private CancellationTokenSource cancellationTokenSource;

        private bool _isInitializing;

        public SearchParameterViewModel(IPhotizerUnitOfWork photizerUnitOfWork
            , IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;
        }

        private async Task Initiliazie()
        {
            PictureCreatedDateFrom = await _photizerUnitOfWork.PictureRepository.GetEarliestCreatedDate().ConfigureAwait(false);
            PictureCreatedDateTo = await _photizerUnitOfWork.PictureRepository.GetLatestCreatedDate().ConfigureAwait(false);
            PictureCreatedTimeTo = new DateTime(PictureCreatedDateTo.Year, PictureCreatedDateTo.Month, PictureCreatedDateTo.Day, 23, 59, 59);
            await UpdateComboboxes().ConfigureAwait(false);
        }

        protected override void OnViewLoaded(object view)
        {
            _isInitializing = true;

            _eventAggregator.SubscribeOnPublishedThread(this);
            cancellationTokenSource = new CancellationTokenSource();
            PictureTags = new ObservableCollection<Tag>();
            PictureTags.CollectionChanged += PictureTags_CollectionChanged;
            PicturePeople = new ObservableCollection<Person>();
            PicturePeople.CollectionChanged += PicturePeople_CollectionChanged;

            Initiliazie().ContinueWith(OnInitializeDone);
        }

        private void OnInitializeDone(Task task)
        {
            _isInitializing = false;
            StartSearch().ContinueWith(OnStartSearchError, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void OnStartSearchError(Task task)
        {
        }

        public async Task HandleAsync(NewPictureWasAddedMessage message, CancellationToken cancellationToken)
        {
            PictureCreatedDateFrom = await Task.Run(() => _photizerUnitOfWork.PictureRepository.GetEarliestCreatedDate()).ConfigureAwait(false);
            PictureCreatedDateTo = await Task.Run(() => _photizerUnitOfWork.PictureRepository.GetLatestCreatedDate()).ConfigureAwait(false);
            PictureCreatedTimeTo = new DateTime(PictureCreatedDateTo.Year, PictureCreatedDateTo.Month, PictureCreatedDateTo.Day, 23, 59, 59);

            await UpdateComboboxes().ConfigureAwait(false);
        }

        private async Task UpdateComboboxes()
        {
            await UpdateCategoryCombobox().ConfigureAwait(false);
            await UpdateCameraCombobox().ConfigureAwait(false);
            await UpdateLenseCombobox().ConfigureAwait(false);
            await UpdateTagsCombobox().ConfigureAwait(false);
            await UpdatePeopleCombobox().ConfigureAwait(false);
            await UpdateLocationCombobox().ConfigureAwait(false);
            await UpdateCountryCombobox().ConfigureAwait(false);
            UpdateRatingParameterCombobox();
        }

        private async Task StartSearch()
        {
            if (!_isInitializing)
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }
                cancellationTokenSource = new CancellationTokenSource();

                DateTime dateTimeFrom = new DateTime(PictureCreatedDateFrom.Year
                    , PictureCreatedDateFrom.Month
                    , PictureCreatedDateFrom.Day
                    , PictureCreatedTimeFrom.Hour
                    , PictureCreatedTimeFrom.Minute
                    , PictureCreatedTimeFrom.Second);

                DateTime dateTimeTo = new DateTime(PictureCreatedDateTo.Year
                    , PictureCreatedDateTo.Month
                    , PictureCreatedDateTo.Day
                    , PictureCreatedTimeTo.Hour
                    , PictureCreatedTimeTo.Minute
                    , PictureCreatedTimeTo.Second);

                var pictureSearcher = IoC.Get<IPictureSearcher>();
                var result = await pictureSearcher.SearchAsync(PictureTags.ToList()
                    , PicturePeople.ToList()
                    , SelectedCategory
                    , SelectedCamera
                    , SelectedLense
                    , SelectedLocation
                    , PictureRating
                    , SelectedRatingParameter
                    , dateTimeFrom
                    , dateTimeTo
                    , SearchTitle
                    , cancellationTokenSource.Token).ConfigureAwait(false);

                if (result != null && result.ToList().Count > 0)
                {
                    await _eventAggregator.PublishOnCurrentThreadAsync(new SearchResultMessage { Pictures = result.ToList() }).ConfigureAwait(false);
                }
                else if (result.ToList().Count == 0)
                {
                    await _eventAggregator.PublishOnCurrentThreadAsync(new SearchResultMessage { Pictures = new List<Picture>() }).ConfigureAwait(true);
                }
            }
        }

        #region Title

        private string _searchTitle;

        public string SearchTitle
        {
            get { return _searchTitle; }
            set
            {
                _searchTitle = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        #endregion Title

        #region Category

        private ObservableCollection<Category> _categories;
        private Category _selectedCategory;

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange();
            }
        }

        private async Task UpdateCategoryCombobox()
        {
            var categories = await _photizerUnitOfWork.CategoryRepository.GetAll().ConfigureAwait(false);
            categories = categories.OrderBy(c => c.Name).ToList();
            Categories = new ObservableCollection<Category>(categories);
        }

        #endregion Category

        #region Rating

        private ObservableCollection<string> _ratingParameters;
        private string _selectedRatingParameter;
        private int _pictureRating;

        public int PictureRating
        {
            get { return _pictureRating; }
            set
            {
                _pictureRating = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        public string SelectedRatingParameter
        {
            get { return _selectedRatingParameter; }
            set
            {
                _selectedRatingParameter = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> RatingParameters
        {
            get { return _ratingParameters; }
            set
            {
                _ratingParameters = value;
                NotifyOfPropertyChange();
            }
        }

        private void UpdateRatingParameterCombobox()
        {
            List<string> parameters = new List<string>
            {
                "<",
                "=",
                ">"
            };
            RatingParameters = new ObservableCollection<string>(parameters);
        }

        #endregion Rating

        #region CreatedDateTime

        private DateTime _pictureCreatedDateFrom;
        private DateTime _pictureCreatedDateTo;
        private DateTime _pictureCreatedTimeFrom;
        private DateTime _pictureCreatedTimeTo;

        public DateTime PictureCreatedTimeTo
        {
            get { return _pictureCreatedTimeTo; }
            set
            {
                _pictureCreatedTimeTo = value;
                Task.Run(() => StartSearch());

                NotifyOfPropertyChange();
            }
        }

        public DateTime PictureCreatedTimeFrom
        {
            get { return _pictureCreatedTimeFrom; }
            set
            {
                _pictureCreatedTimeFrom = value;
                Task.Run(() => StartSearch());

                NotifyOfPropertyChange();
            }
        }

        public DateTime PictureCreatedDateTo
        {
            get { return _pictureCreatedDateTo; }
            set
            {
                _pictureCreatedDateTo = value;
                Task.Run(() => StartSearch());

                NotifyOfPropertyChange();
            }
        }

        public DateTime PictureCreatedDateFrom
        {
            get { return _pictureCreatedDateFrom; }
            set
            {
                _pictureCreatedDateFrom = value;
                Task.Run(() => StartSearch());

                NotifyOfPropertyChange();
            }
        }

        #endregion CreatedDateTime

        #region Camera

        private ObservableCollection<Camera> _cameras;
        private Camera _selectedCamera;

        public Camera SelectedCamera
        {
            get { return _selectedCamera; }
            set
            {
                _selectedCamera = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Camera> Cameras
        {
            get { return _cameras; }
            set
            {
                _cameras = value;
                NotifyOfPropertyChange();
            }
        }

        private async Task UpdateCameraCombobox()
        {
            var cameras = await _photizerUnitOfWork.CameraRepository.GetAll().ConfigureAwait(false);
            cameras = cameras.OrderBy(c => c.Name).ToList();
            Cameras = new ObservableCollection<Camera>(cameras);
        }

        #endregion Camera

        #region Lense

        private ObservableCollection<Lense> _lenses;
        private Lense _selectedLense;

        public Lense SelectedLense
        {
            get { return _selectedLense; }
            set
            {
                _selectedLense = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Lense> Lenses
        {
            get { return _lenses; }
            set
            {
                _lenses = value;
                NotifyOfPropertyChange();
            }
        }

        private async Task UpdateLenseCombobox()
        {
            var lenses = await _photizerUnitOfWork.LenseRepository.GetAll().ConfigureAwait(false);
            lenses = lenses.OrderBy(l => l.Name);
            Lenses = new ObservableCollection<Lense>(lenses);
        }

        #endregion Lense

        #region Country - Location

        private ObservableCollection<string> _countries;
        private string _selectedCountry;
        private ObservableCollection<Location> _locations;
        private Location _selectedLocation;

        public Location SelectedLocation
        {
            get { return _selectedLocation; }
            set
            {
                _selectedLocation = value;
                Task.Run(() => StartSearch());
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Location> Locations
        {
            get { return _locations; }
            set
            {
                _locations = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                _selectedCountry = value;
                Task.Run(UpdateLocationCombobox);
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<string> Countries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                NotifyOfPropertyChange();
            }
        }

        private async Task UpdateCountryCombobox()
        {
            var locations = await _photizerUnitOfWork.LocationRepository.GetAll().ConfigureAwait(false);
            locations = locations.OrderBy(l => l.Country);
            Countries = new ObservableCollection<string>(locations.Select(l => l.Country).ToList().Distinct());
        }

        private async Task UpdateLocationCombobox()
        {
            var locations = await _photizerUnitOfWork.LocationRepository.GetAll().ConfigureAwait(false);
            if (SelectedCountry != null)
            {
                locations = locations.Where(l => l.Country == SelectedCountry).ToList();
                locations = locations.OrderBy(l => l.Place);
                Locations = new ObservableCollection<Location>(locations);
                SelectedLocation = Locations.First();
            }
            else
            {
                Locations = new ObservableCollection<Location>(locations);
            }
        }

        #endregion Country - Location

        #region Tag

        private ObservableCollection<Tag> _tags;
        private ObservableCollection<Tag> _pictureTags;
        private Tag _selectedTag;
        private bool _canAddTag;

        public bool CanAddTag
        {
            get { return _canAddTag; }
            set
            {
                _canAddTag = value;
                NotifyOfPropertyChange();
            }
        }

        public Tag SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                _selectedTag = value;
                if (value != null)
                {
                    CanAddTag = true;
                }
                else
                {
                    CanAddTag = false;
                }
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Tag> Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Tag> PictureTags
        {
            get { return _pictureTags; }
            set
            {
                _pictureTags = value;
                NotifyOfPropertyChange();
            }
        }

        public Task HandleAsync(RemoveTagMessage message, CancellationToken cancellationToken)
        {
            var tag = PictureTags.Where(t => t.Name == message.TagName).FirstOrDefault();
            PictureTags.Remove(tag);
            return Task.CompletedTask;
        }

        public void AddTag()
        {
            if (SelectedTag != null && !PictureTags.Contains(SelectedTag))
            {
                PictureTags.Add(SelectedTag);
            }
        }

        private void PictureTags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Task.Run(() => StartSearch());
        }

        private async Task UpdateTagsCombobox()
        {
            var tags = await _photizerUnitOfWork.TagRepository.GetAll().ConfigureAwait(false);
            tags = tags.OrderBy(t => t.Name).ToList();
            Tags = new ObservableCollection<Tag>(tags);
        }

        #endregion Tag

        #region Person

        private ObservableCollection<Person> _people;
        private ObservableCollection<Person> _picturePeople;
        private Person _selectedPeople;
        private bool _canAddPerson;

        public bool CanAddPerson
        {
            get { return _canAddPerson; }
            set
            {
                _canAddPerson = value;
                NotifyOfPropertyChange();
            }
        }

        public Person SelectedPeople
        {
            get { return _selectedPeople; }
            set
            {
                _selectedPeople = value;
                if (value != null)
                {
                    CanAddPerson = true;
                }
                else
                {
                    CanAddPerson = false;
                }
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Person> PicturePeople
        {
            get { return _picturePeople; }
            set
            {
                _picturePeople = value;
                NotifyOfPropertyChange();
            }
        }

        private void PicturePeople_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Task.Run(() => StartSearch());
        }

        public ObservableCollection<Person> People
        {
            get { return _people; }
            set
            {
                _people = value;
                NotifyOfPropertyChange();
            }
        }

        public Task HandleAsync(RemovePersonMessage message, CancellationToken cancellationToken)
        {
            var person = PicturePeople.Where(p => p.FullName == message.FullName).FirstOrDefault();
            PicturePeople.Remove(person);
            return Task.CompletedTask;
        }

        public void AddPerson()
        {
            if (SelectedPeople != null && !PicturePeople.Contains(SelectedPeople))
            {
                PicturePeople.Add(SelectedPeople);
            }
        }

        private async Task UpdatePeopleCombobox()
        {
            var people = await _photizerUnitOfWork.PersonRepository.GetAll().ConfigureAwait(false);
            people = people.OrderBy(p => p.LastName).ToList();
            People = new ObservableCollection<Person>(people);
        }

        #endregion Person

        public Task HandleAsync(DoSearchWithLocationMessage message, CancellationToken cancellationToken)
        {
            SelectedLocation = message.Location;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
        }

        public async Task HandleAsync(ReloadPictureSearchAfterEditMessage message, CancellationToken cancellationToken)
        {
            await StartSearch().ConfigureAwait(false);
        }

        public async Task HandleAsync(NotifyKeywordDataChangedMessage message, CancellationToken cancellationToken)
        {
            await ReloadRepositories().ConfigureAwait(false);
            await UpdateComboboxes().ConfigureAwait(false);
            await StartSearch().ConfigureAwait(false);

            await _eventAggregator.PublishOnCurrentThreadAsync(new DetailPageKeywordUpdateMessage()).ConfigureAwait(false);
        }

        private async Task ReloadRepositories()
        {
            await _photizerUnitOfWork.TagRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.LenseRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.CameraRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.CategoryRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.PersonRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.LocationRepository.ReloadAll().ConfigureAwait(false);
        }
    }
}