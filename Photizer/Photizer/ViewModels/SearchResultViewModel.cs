using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore.Internal;
using Photizer.Domain.Entities;
using Photizer.Domain.Enums;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
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
using System.Windows.Forms;

namespace Photizer.ViewModels
{
    public class SearchResultViewModel : Caliburn.Micro.Screen, IHandle<SearchResultMessage>, IHandle<PictureResultSelectionChangedMessage>
        , IHandle<CollectionsUpdatedMessage>

    {
        public List<Picture> SearchResult { get; set; }
        public int CurrentPage { get; set; }

        private readonly IEventAggregator _eventAggregator;
        private readonly IBitmapImageResizer _bitmapResizer;
        private readonly IPictureFileManager _pictureFileManager;
        private readonly IPhotizerLogger _logger;
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private bool _canNavPrevPage;
        private bool _canNavNextPage;
        private ObservableCollection<ImageData> _pictures;
        private ObservableCollection<int> _picturesPerPage;
        private int _selectedPicturesPerPage;
        private bool _canExportToFolder;
        private bool _canAddToCollection;
        private ObservableCollection<Collection> _collections;
        private Collection _selectedCollection;
        private SnackbarMessageQueue _messageQueue;

        public SnackbarMessageQueue MessageQueue
        {
            get { return _messageQueue; }
            set
            {
                _messageQueue = value;
                NotifyOfPropertyChange();
            }
        }

        public Collection SelectedCollection
        {
            get { return _selectedCollection; }
            set
            {
                _selectedCollection = value;
                if (value != null)
                {
                    Task.Run(CheckCanAddToCollection);
                }
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<Collection> Collections
        {
            get { return _collections; }
            set
            {
                _collections = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<string> _resultSortCategories;

        public ObservableCollection<string> ResultSortCategories
        {
            get { return _resultSortCategories; }
            set
            {
                _resultSortCategories = value;
                NotifyOfPropertyChange();
            }
        }

        private string _selectedResultSortCategory;

        public string SelectedResultSortCategory
        {
            get { return _selectedResultSortCategory; }
            set
            {
                _selectedResultSortCategory = value;
                if (value != null)
                {
                    Task.Run(() => SortSearchResult());
                }
                NotifyOfPropertyChange();
            }
        }

        public bool CanAddToCollection
        {
            get { return _canAddToCollection; }
            set
            {
                _canAddToCollection = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _exportProgressVisibility;

        public Visibility ExportProgressVisibility
        {
            get { return _exportProgressVisibility; }
            set
            {
                _exportProgressVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanExportToFolder
        {
            get { return _canExportToFolder; }
            set
            {
                _canExportToFolder = value;
                NotifyOfPropertyChange();
            }
        }

        public int SelectedPicturesPerPage
        {
            get { return _selectedPicturesPerPage; }
            set
            {
                if (value != _selectedPicturesPerPage && SearchResult != null)
                {
                    _selectedPicturesPerPage = value;
                    Task.Run(LoadImages);
                    UpdateNavButtonVisibility();
                }
                else
                {
                    _selectedPicturesPerPage = value;
                }
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<int> PicturesPerPage
        {
            get { return _picturesPerPage; }
            set
            {
                _picturesPerPage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanNavNextPage
        {
            get { return _canNavNextPage; }
            set
            {
                _canNavNextPage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanNavPrevPage
        {
            get { return _canNavPrevPage; }
            set
            {
                _canNavPrevPage = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<ImageData> Pictures
        {
            get { return _pictures; }
            set
            {
                _pictures = value;
                NotifyOfPropertyChange();
            }
        }

        public SearchResultViewModel(IEventAggregator eventAggregator
            , IBitmapImageResizer bitmapImageResizer
            , IPictureFileManager pictureFileManager
            , IPhotizerUnitOfWork photizerUnitOfWork
            , IPhotizerLogger logger)
        {
            _eventAggregator = eventAggregator;
            _bitmapResizer = bitmapImageResizer;
            _pictureFileManager = pictureFileManager;
            _photizerUnitOfWork = photizerUnitOfWork;
            _logger = logger;
            _eventAggregator.SubscribeOnPublishedThread(this);

            PicturesPerPage = new ObservableCollection<int>(new List<int> { 9, 15, 30, 60 });
            MessageQueue = new SnackbarMessageQueue();
            Pictures = new ObservableCollection<ImageData>();
            SelectedPicturesPerPage = PicturesPerPage.FirstOrDefault();

            ResultSortCategories = new ObservableCollection<string>
            {
                Multilang.ResultSortCategory_CreatedDateAscending,
                Multilang.ResultSortCategory_CreatedDateDescending,
                Multilang.ResultSortCategory_RatingAscending,
                Multilang.ResultSortCategory_RatingDescending,
                Multilang.ResultSortCategory_PhotizerAddedAscending,
                Multilang.ResultSortCategory_PhotizerAddedDescending
            };
        }

        protected override async void OnViewLoaded(object view)
        {
            CanExportToFolder = false;
            CanAddToCollection = false;
            ExportProgressVisibility = Visibility.Hidden;

            if (Pictures != null && Pictures.Count > 0)
            {
                foreach (var picture in Pictures)
                {
                    await _photizerUnitOfWork.PictureRepository.Reload(picture.Picture).ConfigureAwait(true);
                    NotifyOfPropertyChange(() => Pictures);
                }
            }

            Collections = new ObservableCollection<Collection>(await _photizerUnitOfWork.CollectionRepository.GetAll().ConfigureAwait(false));
        }

        private async Task CheckCanAddToCollection()
        {
            var selectedPictureIds = Pictures.Where(pic => pic.IsSelected == true).Select(p => p.Picture.Id).ToList();

            if (SelectedCollection != null && selectedPictureIds.Count > 0)
            {
                var collections = await _photizerUnitOfWork.CollectionRepository.GetAllWithPictures();
                var selectedCollection = collections.Where(c => c.Id == SelectedCollection.Id).FirstOrDefault();
                var pictureIds = selectedCollection.CollectionPictures.Select(cp => cp.Picture.Id).ToList();
                var pictureIdsNotInCollection = selectedPictureIds.Where(p => !pictureIds.Contains(p)).ToList();

                if (pictureIdsNotInCollection.Count > 0)
                {
                    CanAddToCollection = true;
                }
            }
            else
            {
                CanAddToCollection = false;
            }
        }

        public async Task NavPrevPage()
        {
            CurrentPage--;
            UpdateNavButtonVisibility();
            await LoadImages().ConfigureAwait(true);
        }

        public async Task NavNextPage()
        {
            CurrentPage++;
            UpdateNavButtonVisibility();
            await LoadImages().ConfigureAwait(true);
        }

        private void UpdateNavButtonVisibility()
        {
            if (CurrentPage == 0)
            {
                CanNavPrevPage = false;
            }
            else if (CurrentPage > 0)
            {
                CanNavPrevPage = true;
            }
            if (SearchResult.Count > SelectedPicturesPerPage * (CurrentPage + 1))
            {
                CanNavNextPage = true;
            }
            else
            {
                CanNavNextPage = false;
            }
        }

        public async Task OpenDetailPage(ImageData image)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToDetailPageMessage());

            await _eventAggregator.PublishOnCurrentThreadAsync(new OpenDetailPagePictureMessage() { Picture = image.Picture, CameFromCollection = false });
        }

        public async Task HandleAsync(SearchResultMessage message, CancellationToken cancellationToken)
        {
            SearchResult = message.Pictures;
            CurrentPage = 0;
            UpdateNavButtonVisibility();
            if (message.Pictures.Count > 0)
            {
                await LoadImages().ConfigureAwait(false);
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(delegate
                {
                    Pictures.Clear();
                });
            }
        }

        private async Task SortSearchResult()
        {
            if (SelectedResultSortCategory == Multilang.ResultSortCategory_CreatedDateAscending)
            {
                SearchResult = SearchResult.OrderBy(p => p.Created).ToList();
            }
            else if (SelectedResultSortCategory == Multilang.ResultSortCategory_CreatedDateDescending)
            {
                SearchResult = SearchResult.OrderByDescending(p => p.Created).ToList();
            }
            else if (SelectedResultSortCategory == Multilang.ResultSortCategory_PhotizerAddedAscending)
            {
                SearchResult = SearchResult.OrderBy(p => p.Id).ToList();
            }
            else if (SelectedResultSortCategory == Multilang.ResultSortCategory_PhotizerAddedDescending)
            {
                SearchResult = SearchResult.OrderByDescending(p => p.Id).ToList();
            }
            else if (SelectedResultSortCategory == Multilang.ResultSortCategory_RatingAscending)
            {
                SearchResult = SearchResult.OrderBy(p => p.Rating).ToList();
            }
            else if (SelectedResultSortCategory == Multilang.ResultSortCategory_RatingDescending)
            {
                SearchResult = SearchResult.OrderByDescending(p => p.Rating).ToList();
            }

            await LoadImages().ConfigureAwait(false);
        }

        private async Task LoadImages()
        {
            Pictures = new ObservableCollection<ImageData>();
            var currentPictures = SearchResult.Skip(SelectedPicturesPerPage * CurrentPage).Take(SelectedPicturesPerPage);
            foreach (var pic in currentPictures)
            {
                try
                {
                    var imageFile = _pictureFileManager.GetPictureFileByPicture(pic);
                    if (imageFile != null)
                    {
                        var image = await _bitmapResizer.GetResizedBitmapImage(imageFile.FullName, ImageSize.Medium);
                        if (image != null)
                        {
                            ImageData newImg = new ImageData
                            {
                                FilePath = _pictureFileManager.GetPictureFileByPicture(pic).FullName,
                                Picture = pic,
                                Image = image
                            };
                            System.Windows.Application.Current.Dispatcher.Invoke(delegate
                            {
                                Pictures.Add(newImg);
                            });
                        }
                    }
                    else
                    {
                        //Image manually removed - maybe restore from backup or delete from database
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while loading Pictures", ex);
                }
            }
        }

        public async Task ExportToFolder()
        {
            var picturesToExport = Pictures.Where(pic => pic.IsSelected == true).ToList();
            using var folderBrowser = new FolderBrowserDialog();
            var result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                string exportFolder = folderBrowser.SelectedPath + @"\";
                ExportProgressVisibility = Visibility.Visible;
                foreach (var picture in picturesToExport)
                {
                    await _pictureFileManager.ExportPictureAsync(new DirectoryInfo(exportFolder), picture.Picture).ConfigureAwait(false);
                }
                ExportProgressVisibility = Visibility.Hidden;
            }
        }

        public async Task AddToCollection()
        {
            var picturesToAdd = Pictures.Where(pic => pic.IsSelected == true).Select(p => p.Picture).ToList();
            var selectedPictureIds = Pictures.Where(pic => pic.IsSelected == true).Select(p => p.Picture.Id).ToList();

            var collections = await _photizerUnitOfWork.CollectionRepository.GetAllWithPictures();
            var selectedCollection = collections.Where(c => c.Id == SelectedCollection.Id).FirstOrDefault();
            var pictureIds = selectedCollection.CollectionPictures.Select(cp => cp.Picture.Id).ToList();
            var pictureIdsNotInCollection = selectedPictureIds.Where(p => !pictureIds.Contains(p)).ToList();

            picturesToAdd = picturesToAdd.Where(p => pictureIdsNotInCollection.Contains(p.Id)).ToList();

            foreach (var picture in picturesToAdd)
            {
                await _photizerUnitOfWork.PictureRepository.AddToCollection(picture, SelectedCollection);
            }
            await _photizerUnitOfWork.Save();
            MessageQueue.Enqueue("Added to Collection");
        }

        public async Task HandleAsync(PictureResultSelectionChangedMessage message, CancellationToken cancellationToken)
        {
            if (Pictures.Any(pic => pic.IsSelected == true))
            {
                CanExportToFolder = true;
            }
            else
            {
                CanExportToFolder = false;
            }
            await CheckCanAddToCollection();
        }

        public async Task HandleAsync(CollectionsUpdatedMessage message, CancellationToken cancellationToken)
        {
            Collections = new ObservableCollection<Collection>(await _photizerUnitOfWork.CollectionRepository.GetAll());
        }
    }
}