using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Win32;
using Photizer.Commands;
using Photizer.DialogViewModels;
using Photizer.DialogViews;
using Photizer.Domain.Entities;
using Photizer.Domain.Enums;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.Domain.Models;
using Photizer.ImageUtilities;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Photizer.ViewModels
{
    public class AddPictureViewModel : Screen, IHandle<RemoveTagMessage>, IHandle<RemovePersonMessage>, IHandle<AddPictureSettingsMessage>
        , IHandle<KeyDownEventMessage<Category>>, IHandle<KeyDownEventMessage<Camera>>, IHandle<KeyDownEventMessage<Lense>>, IHandle<KeyDownEventMessage<Tag>>
    {
        public ICommand OpenAddNewPersonDialogCommand => new DialogCommand(RunAddNewPersonDialog);
        public ICommand OpenAddNewLocationDialogCommand => new DialogCommand(RunAddNewLocationDialog);
        public ICommand OpenAddPictureSettingsDialogCommand => new DialogCommand(RunAddPictureSettingsDialog);

        public static int ButtonHeight => 30;

        private readonly IEventAggregator _eventAggregator;
        private readonly IBitmapImageResizer _bitmapResizer;
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IPictureFileManager _pictureFileManager;
        private readonly IExifDataExtractor _exifDataExtractor;

        private Visibility _thumbnailLoadingProgressVisibility;
        private Visibility _detailGridVisibility;
        private ImageData _selectedPictureThumbnail;
        private ObservableCollection<ImageData> _pictureThumbnails;
        private bool _canSavePicture;
        private ExifData _exifData;
        private AddPictureSettings _addPictureSettings;

        private readonly IPhotizerLogger _logger;

        public AddPictureSettings AddPictureSettings
        {
            get { return _addPictureSettings; }
            set
            {
                _addPictureSettings = value;
                NotifyOfPropertyChange();
            }
        }

        public ExifData ExifData
        {
            get { return _exifData; }
            set
            {
                _exifData = value;
                if (value != null)
                {
                    UpdatePictureDataFromExif().ContinueWith(OnUpdatePictureDataFromExifError, TaskContinuationOptions.OnlyOnFaulted);
                }
                NotifyOfPropertyChange();
            }
        }

        public bool CanSavePicture
        {
            get { return _canSavePicture; }
            set
            {
                _canSavePicture = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility ThumbnailLoadingProgressVisibility
        {
            get { return _thumbnailLoadingProgressVisibility; }
            set
            {
                _thumbnailLoadingProgressVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility DetailGridVisibility
        {
            get { return _detailGridVisibility; }
            set
            {
                _detailGridVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<ImageData> PictureThumbnails
        {
            get => _pictureThumbnails;
            set
            {
                _pictureThumbnails = value;
                NotifyOfPropertyChange();
            }
        }

        public ImageData SelectedPictureThumbnail
        {
            get { return _selectedPictureThumbnail; }
            set
            {
                if (value != null)
                {
                    SelectedCamera = null;
                    SelectedLense = null;
                    _selectedPictureThumbnail = value;
                    InitializeImage(value.FilePath).ContinueWith(OnInitializeImageError, TaskContinuationOptions.OnlyOnFaulted);
                    DetailGridVisibility = Visibility.Visible;
                }
                else
                {
                    DetailGridVisibility = Visibility.Collapsed;
                    _selectedPictureThumbnail = value;
                }
                NotifyOfPropertyChange();
            }
        }

        private async Task InitializeImage(string filePath)
        {
            try
            {
                SelectedPictureThumbnail.Image = await _bitmapResizer.GetResizedBitmapImage(filePath, ImageSize.Big).ConfigureAwait(true);
                ExifData = _exifDataExtractor.ExtractExifData(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError("Loading Image failed", ex);
            }
        }

        private void OnInitializeImageError(Task task)
        {
            _logger.LogError("InitializeImage Error", task.Exception);
        }

        private void OnUpdatePictureDataFromExifError(Task task)
        {
            _logger.LogError("UpdatePictureDataFromExif Error", task.Exception);
        }

        public AddPictureViewModel(IBitmapImageResizer bitmapResizer
            , IPhotizerUnitOfWork photizerUnitOfWork
            , IPictureFileManager pictureFileManager
            , IExifDataExtractor exifDataExtractor
            , IEventAggregator eventAggregator, IPhotizerLogger logger)
        {
            _bitmapResizer = bitmapResizer;
            _photizerUnitOfWork = photizerUnitOfWork;
            _pictureFileManager = pictureFileManager;
            _eventAggregator = eventAggregator;
            _exifDataExtractor = exifDataExtractor;

            _eventAggregator.SubscribeOnPublishedThread(this);

            PictureCreatedDate = new DateTime(DateTime.Now.Year, 1, 1);
            ThumbnailLoadingProgressVisibility = Visibility.Collapsed;
            DetailGridVisibility = Visibility.Collapsed;
            CategoryChipVisibility = Visibility.Collapsed;
            CameraChipVisibility = Visibility.Collapsed;
            LenseChipVisibility = Visibility.Collapsed;
            LocationChipVisibility = Visibility.Collapsed;

            PictureTags = new ObservableCollection<Tag>();
            PicturePeople = new ObservableCollection<Person>();

            AddPictureSettings = new AddPictureSettings();
            _logger = logger;
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

        public Task HandleAsync(RemoveTagMessage message, CancellationToken cancellationToken)
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

        private async Task RunAddNewPersonDialog()
        {
            var view = new AddNewPersonDialogView
            {
                DataContext = IoC.Get<AddNewPersonDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await UpdatePeopleCombobox().ConfigureAwait(false);
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

        private async Task RunAddNewLocationDialog()
        {
            var view = new AddNewLocationDialogView
            {
                DataContext = IoC.Get<AddNewLocationDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await UpdateLocationCombobox().ConfigureAwait(false);
            await UpdateCountryCombobox().ConfigureAwait(false);
        }

        #endregion Location

        #region SavePicture

        public async Task SavePicture()
        {
            FileInfo pictureFile = new FileInfo(SelectedPictureThumbnail.FilePath);

            Picture picture = new Picture
            {
                Created = new DateTime(PictureCreatedDate.Year
                    , PictureCreatedDate.Month
                    , PictureCreatedDate.Day
                    , PictureCreatedTime.Hour
                    , PictureCreatedTime.Minute
                    , PictureCreatedTime.Second),
                Rating = PictureRating,
                Title = PictureTitle,
                FileType = pictureFile.Extension,
                ShutterSpeed = ExifData.ShutterSpeed,
                Aperture = ExifData.Aperture,
                FocalLength = ExifData.FocalLength,
                ISOSpeed = ExifData.ISOSpeed,
            };
            picture.Camera = SelectedCamera;
            picture.Lense = SelectedLense;
            picture.Category = SelectedCategory;
            picture.Location = SelectedLocation;
            picture.PicturePeople = new List<PicturePerson>();
            picture.PictureTags = new List<PictureTag>();

            var createdPicture = await _photizerUnitOfWork.PictureRepository.Add(picture).ConfigureAwait(true);
            await _photizerUnitOfWork.Save().ConfigureAwait(true);

            foreach (var tag in PictureTags)
            {
                await _photizerUnitOfWork.PictureRepository.AddTag(createdPicture, tag).ConfigureAwait(true);
            }
            foreach (var person in PicturePeople)
            {
                await _photizerUnitOfWork.PictureRepository.AddPerson(createdPicture, person).ConfigureAwait(true);
            }
            await _photizerUnitOfWork.Save().ConfigureAwait(true);
            await _pictureFileManager.AddPictureFileToManagedFoldersAsync(pictureFile, createdPicture).ConfigureAwait(true);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NewPictureWasAddedMessage()).ConfigureAwait(true);
            CleanUI();
        }

        private void CleanUI()
        {
            try
            {
                PictureThumbnails.Remove(SelectedPictureThumbnail);
                SelectedPictureThumbnail = null;
                PictureTitle = string.Empty;
                PictureRating = 0;
                PictureCreatedDate = new DateTime(DateTime.Now.Year, 1, 1);
                PictureCreatedTime = new DateTime(1, 1, 1, 0, 0, 1);
                SelectedCamera = null;
                SelectedLense = null;
                SelectedTag = null;
                SelectedPeople = null;
                SelectedCountry = null;
                ExifData = null;

                if (!AddPictureSettings.KeepCategory)
                {
                    SelectedCategory = null;
                }

                if (!AddPictureSettings.KeepTags && PictureTags.Count > 0)
                {
                    PictureTags.Clear();
                }

                if (!AddPictureSettings.KeepPeople && PicturePeople.Count > 0)
                {
                    PicturePeople.Clear();
                }

                if (!AddPictureSettings.KeepLocation)
                {
                    SelectedLocation = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error cleaning UI", ex);
            }
        }

        private void CheckCanSavePicture()
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

        #endregion SavePicture

        protected async override void OnViewLoaded(object view)
        {
            await Task.Run(() => UpdateComboboxes()).ConfigureAwait(false);
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

        public async Task UpdatePictureDataFromExif()
        {
            try
            {
                if (ExifData != null)
                {
                    var cameras = await _photizerUnitOfWork.CameraRepository.GetAll().ConfigureAwait(true);
                    var lenses = await _photizerUnitOfWork.LenseRepository.GetAll().ConfigureAwait(true);

                    if (!string.IsNullOrEmpty(ExifData.Camera))
                    {
                        if (!cameras.Any(c => c.Name == ExifData.Camera))
                        {
                            Camera newCamera = new Camera { Name = ExifData.Camera };
                            await _photizerUnitOfWork.CameraRepository.Add(newCamera).ConfigureAwait(true);
                            await _photizerUnitOfWork.Save().ConfigureAwait(true);
                            await UpdateCameraCombobox().ConfigureAwait(true);
                        }
                        SelectedCamera = Cameras.Where(c => c.Name == ExifData.Camera).FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(ExifData.Lense))
                    {
                        if (!lenses.Any(l => l.Name == ExifData.Lense))
                        {
                            Lense newLense = new Lense { Name = ExifData.Lense };
                            await _photizerUnitOfWork.LenseRepository.Add(newLense).ConfigureAwait(true);
                            await _photizerUnitOfWork.Save().ConfigureAwait(true);
                            await UpdateLenseCombobox().ConfigureAwait(true);
                        }
                        SelectedLense = Lenses.Where(l => l.Name == ExifData.Lense).FirstOrDefault();
                    }

                    if (ExifData.Created != null && ExifData.Created.Year != 1)
                    {
                        PictureCreatedDate = new DateTime(ExifData.Created.Year, ExifData.Created.Month, ExifData.Created.Day);
                        PictureCreatedTime = new DateTime(ExifData.Created.Year, ExifData.Created.Month, ExifData.Created.Day, ExifData.Created.Hour, ExifData.Created.Minute, ExifData.Created.Second);
                    }
                    else
                    {
                        PictureCreatedDate = new DateTime(DateTime.Now.Year, 1, 1);
                        PictureCreatedTime = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Updating Picture Data failed {ex.Message}", ex);
            }
        }

        public async Task OpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Bilder (*.jpeg;*.png)|*.png;*.jpeg;*.jpg"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ThumbnailLoadingProgressVisibility = Visibility.Visible;
                var files = openFileDialog.FileNames.ToList();
                await LoadThumbnails(files).ConfigureAwait(false);
                ThumbnailLoadingProgressVisibility = Visibility.Collapsed;
            }
        }

        private async Task LoadThumbnails(List<string> files)
        {
            PictureThumbnails = new ObservableCollection<ImageData>();
            foreach (var file in files)
            {
                ImageData newThumb = new ImageData
                {
                    FilePath = file,

                    Image = await Task.Run(() => _bitmapResizer.GetResizedBitmapImage(file, ImageSize.Small)).ConfigureAwait(true)
                };

                PictureThumbnails.Add(newThumb);
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }

        public Task HandleAsync(AddPictureSettingsMessage message, CancellationToken cancellationToken)
        {
            AddPictureSettings = message.AddPictureSettings;
            return Task.CompletedTask;
        }

        private async Task RunAddPictureSettingsDialog()
        {
            var view = new AddPictureSettingsView
            {
                DataContext = IoC.Get<AddPictureSettingsViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(false);
        }

        public async Task HandleAsync(KeyDownEventMessage<Category> message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.Content) && !Categories.Any(c => c.Name == message.Content))
            {
                Category newCategory = new Category { Name = message.Content };
                await _photizerUnitOfWork.CategoryRepository.Add(newCategory).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await UpdateCategoryCombobox().ConfigureAwait(false);

                SelectedCategory = Categories.Where(c => c.Name == message.Content).FirstOrDefault();
            }
        }

        public async Task HandleAsync(KeyDownEventMessage<Camera> message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.Content) && !Cameras.Any(c => c.Name == message.Content))
            {
                Camera newCamera = new Camera { Name = message.Content };
                await _photizerUnitOfWork.CameraRepository.Add(newCamera).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await UpdateCameraCombobox().ConfigureAwait(false);

                SelectedCamera = Cameras.Where(c => c.Name == message.Content).FirstOrDefault();
            }
        }

        public async Task HandleAsync(KeyDownEventMessage<Lense> message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.Content) && !Lenses.Any(c => c.Name == message.Content))
            {
                Lense newLense = new Lense { Name = message.Content };
                await _photizerUnitOfWork.LenseRepository.Add(newLense).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await UpdateLenseCombobox().ConfigureAwait(false);

                SelectedLense = Lenses.Where(c => c.Name == message.Content).FirstOrDefault();
            }
        }

        public async Task HandleAsync(KeyDownEventMessage<Tag> message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.Content) && !Tags.Any(c => c.Name == message.Content))
            {
                Tag newLense = new Tag { Name = message.Content };
                await _photizerUnitOfWork.TagRepository.Add(newLense).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await UpdateTagsCombobox().ConfigureAwait(false);

                SelectedTag = Tags.Where(c => c.Name == message.Content).FirstOrDefault();
                Application.Current.Dispatcher.Invoke(delegate
                {
                    AddTag();
                });
                SelectedTag = null;
            }
            else
            {
                SelectedTag = Tags.Where(c => c.Name == message.Content).FirstOrDefault();
                Application.Current.Dispatcher.Invoke(delegate
                {
                    AddTag();
                });
                SelectedTag = null;
            }
        }
    }
}