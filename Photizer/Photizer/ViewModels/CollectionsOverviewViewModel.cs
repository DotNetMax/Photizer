using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Photizer.Commands;
using Photizer.DialogViewModels;
using Photizer.DialogViews;
using Photizer.Domain.Enums;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.ImageUtilities;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Photizer.ViewModels
{
    public class CollectionsOverviewViewModel : Screen, IDisposable
    {
        public ICommand OpenAddNewCollectionDialogCommand => new DialogCommand(RunAddNewCollectionDialog);

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBitmapImageResizer _bitmapImageResizer;
        private readonly IPictureFileManager _pictureFileManager;
        private readonly IPhotizerLogger _logger;

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

        private ObservableCollection<CollectionData> _collectionsData;

        public ObservableCollection<CollectionData> CollectionsData
        {
            get { return _collectionsData; }
            set
            {
                _collectionsData = value;
                NotifyOfPropertyChange();
            }
        }

        public CollectionsOverviewViewModel(IPhotizerUnitOfWork photizerUnitOfWork
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
            CollectionsData = new ObservableCollection<CollectionData>();
            MessageQueue = new SnackbarMessageQueue();
        }

        protected override void OnViewLoaded(object view)
        {
            LoadCollections().ConfigureAwait(false);
            base.OnViewLoaded(view);
        }

        public async Task OpenCollectionsDetailView(CollectionData collectionData)
        {
            await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToCollectionDetailsMessage()).ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new OpenCollectionDetailsMessage() { Collection = collectionData.Collection }).ConfigureAwait(false);
        }

        public async Task DeleteCollection(CollectionData collectionData)
        {
            var result = await _photizerUnitOfWork.CollectionRepository.Delete(collectionData.Collection).ConfigureAwait(true);
            await _photizerUnitOfWork.Save().ConfigureAwait(false);
            if (result)
            {
                MessageQueue.Enqueue("Collection deleted");
            }
            else
            {
                MessageQueue.Enqueue("Collection could not be deleted");
            }
            await LoadCollections().ConfigureAwait(false);
        }

        private async Task RunAddNewCollectionDialog()
        {
            var view = new AddNewCollectionDialogView
            {
                DataContext = IoC.Get<AddNewCollectionDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await LoadCollections().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new CollectionsUpdatedMessage()).ConfigureAwait(false);
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }

        private async Task LoadCollections()
        {
            CollectionsData = new ObservableCollection<CollectionData>();
            var collections = await _photizerUnitOfWork.CollectionRepository.GetAllWithPictures().ConfigureAwait(false);
            foreach (var collection in collections)
            {
                try
                {
                    if (collection.CollectionPictures.Count > 0)
                    {
                        var picture = collection.CollectionPictures.Select(cp => cp.Picture).FirstOrDefault();
                        var image = await Task.Run(
                            () => _bitmapImageResizer.GetResizedBitmapImage(
                                _pictureFileManager.GetPictureFileByPicture(picture).FullName, ImageSize.Medium)).ConfigureAwait(true);
                        if (image != null)
                        {
                            CollectionData newImg = new CollectionData
                            {
                                Collection = collection,
                                Image = image
                            };
                            System.Windows.Application.Current.Dispatcher.Invoke(delegate
                            {
                                CollectionsData.Add(newImg);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while loading Pictures", ex);
                }
            }
        }

        public void Dispose()
        {
            MessageQueue?.Dispose();
        }
    }
}