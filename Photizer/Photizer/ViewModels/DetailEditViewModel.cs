using Caliburn.Micro;
using Microsoft.EntityFrameworkCore.Internal;
using Photizer.Domain.Entities;
using Photizer.Domain.EventMessages;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Photizer.ViewModels
{
    public class DetailEditViewModel : Screen, IHandle<RemoveDetailEditPersonMessage>, IHandle<RemoveDetailEditTagMessage>, IHandle<InitDetailEditViewModelMessage>
    {
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;

        public Picture Picture { get; set; }
        public static int ButtonHeight => 30;

        private bool _isInitializing;

        public DetailEditViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;

            PictureTags = new ObservableCollection<Tag>();
            PicturePeople = new ObservableCollection<Person>();

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        #region Title

        private string _pictureTitle;

        public string PictureTitle
        {
            get { return _pictureTitle; }
            set
            {
                _pictureTitle = value;
                CheckCanSavePicture();
                NotifyOfPropertyChange();
            }
        }

        #endregion Title

        #region Rating-CreatedDateTime

        private int _pictureRating;
        private DateTime _pictureCreatedDate;
        private DateTime _pictureCreatedTime;

        public int PictureRating
        {
            get { return _pictureRating; }
            set
            {
                _pictureRating = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime PictureCreatedDate
        {
            get { return _pictureCreatedDate; }
            set
            {
                _pictureCreatedDate = value;
                CheckCanSavePicture();
                NotifyOfPropertyChange();
            }
        }

        public DateTime PictureCreatedTime
        {
            get { return _pictureCreatedTime; }
            set
            {
                _pictureCreatedTime = value;
                CheckCanSavePicture();
                NotifyOfPropertyChange();
            }
        }

        #endregion Rating-CreatedDateTime

        #region Category

        private Visibility _categoryChipVisibility;
        private Category _selectedCategory;
        private ObservableCollection<Category> _categories;

        public Visibility CategoryChipVisibility
        {
            get { return _categoryChipVisibility; }
            set
            {
                _categoryChipVisibility = value;
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

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                if (value != null)
                {
                    CategoryChipVisibility = Visibility.Visible;
                }
                else
                {
                    CategoryChipVisibility = Visibility.Collapsed;
                }
                CheckCanSavePicture();
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

        #region Camera

        private Visibility _cameraChipVisibility;
        private Camera _selectedCamera;
        private ObservableCollection<Camera> _cameras;

        public Visibility CameraChipVisibility
        {
            get { return _cameraChipVisibility; }
            set
            {
                _cameraChipVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public Camera SelectedCamera
        {
            get { return _selectedCamera; }
            set
            {
                _selectedCamera = value;
                if (value != null)
                {
                    CameraChipVisibility = Visibility.Visible;
                }
                else
                {
                    CameraChipVisibility = Visibility.Collapsed;
                }
                CheckCanSavePicture();
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

        private Visibility _lenseChipVisibility;
        private Lense _selectedLense;
        private ObservableCollection<Lense> _lenses;

        public Visibility LenseChipVisibility
        {
            get { return _lenseChipVisibility; }
            set
            {
                _lenseChipVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public Lense SelectedLense
        {
            get { return _selectedLense; }
            set
            {
                _selectedLense = value;
                if (value != null)
                {
                    LenseChipVisibility = Visibility.Visible;
                }
                else
                {
                    LenseChipVisibility = Visibility.Collapsed;
                }
                CheckCanSavePicture();
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

        public Task HandleAsync(RemoveDetailEditTagMessage message, CancellationToken cancellationToken)
        {
            var tag = PictureTags.Where(t => t.Name == message.TagName).FirstOrDefault();
            PictureTags.Remove(tag);
            CheckCanSavePicture();
            return Task.CompletedTask;
        }

        public void AddTag()
        {
            if (SelectedTag != null && !PictureTags.Contains(SelectedTag))
            {
                PictureTags.Add(SelectedTag);
            }
            CheckCanSavePicture();
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

        public ObservableCollection<Person> People
        {
            get { return _people; }
            set
            {
                _people = value;
                NotifyOfPropertyChange();
            }
        }

        public Task HandleAsync(RemoveDetailEditPersonMessage message, CancellationToken cancellationToken)
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

        #region Location

        private Visibility _locationChipVisibility;
        private ObservableCollection<string> _countries;
        private ObservableCollection<Location> _locations;
        private Location _selectedLocation;
        private string _selectedCountry;

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

        public Location SelectedLocation
        {
            get { return _selectedLocation; }
            set
            {
                _selectedLocation = value;
                if (value != null)
                {
                    LocationChipVisibility = Visibility.Visible;
                }
                else
                {
                    LocationChipVisibility = Visibility.Collapsed;
                }
                CheckCanSavePicture();
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

        public ObservableCollection<string> Countries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility LocationChipVisibility
        {
            get { return _locationChipVisibility; }
            set
            {
                _locationChipVisibility = value;
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
            }
            else
            {
                Locations = new ObservableCollection<Location>(locations);
            }
        }

        #endregion Location

        private void CheckCanSavePicture()
        {
            if (!_isInitializing)
            {
                if (PictureCreatedDate != null
                    && PictureCreatedTime != null
                    && SelectedCategory != null
                    && SelectedCamera != null
                    && PictureTags.Count > 0
                    && SelectedLocation != null)
                {
                    CanSavePicture = true;
                }
                else
                {
                    CanSavePicture = false;
                }
            }
        }

        private bool _canSavePicture;

        public bool CanSavePicture
        {
            get { return _canSavePicture; }
            set
            {
                _canSavePicture = value;
                NotifyOfPropertyChange();
            }
        }

        public async Task Cancel()
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new DetailWasEditedMessage() { Picture = Picture }).ConfigureAwait(false);
        }

        public async Task SavePicture()
        {
            Picture.Title = PictureTitle;
            Picture.Rating = PictureRating;
            Picture.Created = new DateTime(PictureCreatedDate.Year
                    , PictureCreatedDate.Month
                    , PictureCreatedDate.Day
                    , PictureCreatedTime.Hour
                    , PictureCreatedTime.Minute
                    , PictureCreatedTime.Second);
            Picture.Category = SelectedCategory;
            Picture.Lense = SelectedLense;
            Picture.Camera = SelectedCamera;
            Picture.Location = SelectedLocation;

            await _photizerUnitOfWork.Save().ConfigureAwait(false);
            await UpdateTagsAndPeople().ConfigureAwait(false);
            var picture = await _photizerUnitOfWork.PictureRepository.GetPictureWithAllDataById(Picture.Id).ConfigureAwait(false);

            await _eventAggregator.PublishOnCurrentThreadAsync(new DetailWasEditedMessage() { Picture = picture }).ConfigureAwait(false);
        }

        private async Task UpdateTagsAndPeople()
        {
            foreach (var tag in PictureTags)
            {
                if (!Picture.PictureTags.Any(pt => pt.Tag == tag))
                {
                    await _photizerUnitOfWork.PictureRepository.AddTag(Picture, tag).ConfigureAwait(true);
                }
            }
            foreach (var tag in Picture.PictureTags.Select(pt => pt.Tag).ToList())
            {
                if (!PictureTags.Any(t => t == tag))
                {
                    await _photizerUnitOfWork.PictureRepository.RemoveTag(Picture, tag).ConfigureAwait(true);
                }
            }

            foreach (var person in PicturePeople)
            {
                if (!Picture.PicturePeople.Any(pp => pp.Person == person))
                {
                    await _photizerUnitOfWork.PictureRepository.AddPerson(Picture, person).ConfigureAwait(true);
                }
            }
            foreach (var person in Picture.PicturePeople.Select(pp => pp.Person).ToList())
            {
                if (!PicturePeople.Any(p => p == person))
                {
                    await _photizerUnitOfWork.PictureRepository.RemovePerson(Picture, person).ConfigureAwait(true);
                }
            }
            await _photizerUnitOfWork.Save().ConfigureAwait(true);
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
        }

        public async Task HandleAsync(InitDetailEditViewModelMessage message, CancellationToken cancellationToken)
        {
            _isInitializing = true;
            await UpdateComboboxes().ConfigureAwait(false);
            var picture = await _photizerUnitOfWork.PictureRepository.GetPictureWithAllDataById(message.PictureId).ConfigureAwait(false);

            Picture = picture;
            PictureTitle = picture.Title;
            PictureRating = picture.Rating;
            PictureCreatedDate = picture.Created.Date;
            PictureCreatedTime = picture.Created;
            SelectedCategory = picture.Category;
            SelectedCamera = picture.Camera;
            SelectedLense = picture.Lense;
            SelectedLocation = picture.Location;
            PictureTags = new ObservableCollection<Tag>(picture.PictureTags.Select(pt => pt.Tag));
            PicturePeople = new ObservableCollection<Person>(picture.PicturePeople.Select(pp => pp.Person));
            _isInitializing = false;

            CheckCanSavePicture();
        }
    }
}