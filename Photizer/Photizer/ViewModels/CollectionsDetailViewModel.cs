using Caliburn.Micro;
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
    public class CollectionsDetailViewModel : Caliburn.Micro.Screen, IHandle<OpenCollectionDetailsMessage>, IHandle<PictureResultSelectionChangedMessage>
    {
        private List<Picture> _collectionPictures;
        private Collection _collection;

        public int CurrentPage { get; set; }

        private readonly IEventAggregator _eventAggregator;
        private readonly IBitmapImageResizer _bitmapImageResizer;
        private readonly IPictureFileManager _pictureFileManager;
        private readonly IPhotizerLogger _logger;
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private bool _canNavPrevPage;
        private bool _canNavNextPage;
        private ObservableCollection<ImageData> _pictures;
        private ObservableCollection<int> _picturesPerPage;
        private int _selectedPicturesPerPage;
        private bool _canExportToFolder;
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
                if (value != _selectedPicturesPerPage && _collectionPictures != null)
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

        public CollectionsDetailViewModel(IPhotizerUnitOfWork photizerUnitOfWork
            , IEventAggregator eventAggregator
            , IBitmapImageResizer bitmapImageResizer
            , IPictureFileManager pictureFileManager
            , IPhotizerLogger logger)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;
            _bitmapImageResizer = bitmapImageResizer;
            _pictureFileManager = pictureFileManager;
            _logger = logger;

            _eventAggregator.SubscribeOnPublishedThread(this);
            PicturesPerPage = new ObservableCollection<int>(new List<int> { 9, 15, 30, 60 });
            SelectedPicturesPerPage = PicturesPerPage.FirstOrDefault();
            _collectionPictures = new List<Picture>();
        }

        protected override void OnViewLoaded(object view)
        {
            CanExportToFolder = false;
            ExportProgressVisibility = Visibility.Hidden;

            UpdateNavButtonVisibility();
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

        public async Task OpenDetailPage(ImageData image)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToDetailPageMessage()).ConfigureAwait(true);

            await _eventAggregator.PublishOnCurrentThreadAsync(new OpenDetailPagePictureMessage()
            { Picture = image.Picture, CameFromCollection = true, Collection = _collection }).ConfigureAwait(true);
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
            if (_collectionPictures.Count > SelectedPicturesPerPage * (CurrentPage + 1))
            {
                CanNavNextPage = true;
            }
            else
            {
                CanNavNextPage = false;
            }
        }

        private async Task LoadImages()
        {
            Pictures = new ObservableCollection<ImageData>();
            var currentPictures = _collectionPictures.Skip(SelectedPicturesPerPage * CurrentPage).Take(SelectedPicturesPerPage);
            foreach (var pic in currentPictures)
            {
                try
                {
                    var image = await Task.Run(
                        () => _bitmapImageResizer.GetResizedBitmapImage(
                            _pictureFileManager.GetPictureFileByPicture(pic).FullName, ImageSize.Medium)).ConfigureAwait(true);
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
                catch (Exception ex)
                {
                    _logger.LogError("Error while loading Pictures", ex);
                }
            }
        }

        public Task HandleAsync(PictureResultSelectionChangedMessage message, CancellationToken cancellationToken)
        {
            if (Pictures.Any(pic => pic.IsSelected == true))
            {
                CanExportToFolder = true;
            }
            else
            {
                CanExportToFolder = false;
            }
            return Task.CompletedTask;
        }

        public async Task HandleAsync(OpenCollectionDetailsMessage message, CancellationToken cancellationToken)
        {
            _collectionPictures = new List<Picture>();
            _collection = await _photizerUnitOfWork.CollectionRepository.GetWithPictures(message.Collection.Id).ConfigureAwait(false);
            _collectionPictures = _collection.CollectionPictures.Select(cp => cp.Picture).ToList();

            foreach (var picture in _collectionPictures)
            {
                await _photizerUnitOfWork.PictureRepository.Reload(picture).ConfigureAwait(false);
            }

            await LoadImages().ConfigureAwait(true);
        }

        public async Task NavigateBackToCollections()
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateBackToCollectionsMessage()).ConfigureAwait(false);
        }
    }
}