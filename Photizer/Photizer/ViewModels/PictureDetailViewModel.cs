using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Domain.Enums;
using Photizer.Domain.EventMessages;
using Photizer.Domain.Interfaces;
using Photizer.ImageUtilities;
using Photizer.Infrastructure.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Photizer.ViewModels
{
    public class PictureDetailViewModel : Screen, IHandle<OpenDetailPagePictureMessage>, IHandle<DetailWasEditedMessage>, IHandle<DetailPageKeywordUpdateMessage>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IBitmapImageResizer _bitmapImageResizer;
        private readonly IPictureFileManager _pictureFileManager;
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private bool _cameFromCollection;
        private Collection _sourceCollection;

        private DetailEditViewModel _detailEditViewModel;

        public DetailEditViewModel DetailEditViewModel
        {
            get { return _detailEditViewModel; }
            set
            {
                _detailEditViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _editViewVisibility;

        public Visibility EditViewVisibility
        {
            get { return _editViewVisibility; }
            set
            {
                _editViewVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _detailMenuVisibility;

        public Visibility DetailMenuVisibility
        {
            get { return _detailMenuVisibility; }
            set
            {
                _detailMenuVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private BitmapImage _image;

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyOfPropertyChange();
            }
        }

        private Picture _picture;

        public Picture Picture
        {
            get { return _picture; }
            set
            {
                _picture = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<Tag> _tags;

        public ObservableCollection<Tag> Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<Person> _people;

        public ObservableCollection<Person> People
        {
            get { return _people; }
            set
            {
                _people = value;
                NotifyOfPropertyChange();
            }
        }

        public PictureDetailViewModel(IEventAggregator eventAggregator
            , IBitmapImageResizer bitmapImageResizer
            , IPictureFileManager pictureFileManager
            , IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _eventAggregator = eventAggregator;
            _bitmapImageResizer = bitmapImageResizer;
            _pictureFileManager = pictureFileManager;
            _photizerUnitOfWork = photizerUnitOfWork;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public async Task HandleAsync(OpenDetailPagePictureMessage message, CancellationToken cancellationToken)
        {
            Image = await Task.Run(() => _bitmapImageResizer.GetResizedBitmapImage(_pictureFileManager.GetPictureFileByPicture(message.Picture).FullName, ImageSize.Original)).ConfigureAwait(false);
            Picture = await _photizerUnitOfWork.PictureRepository.GetPictureWithAllDataById(message.Picture.Id).ConfigureAwait(false);
            Tags = new ObservableCollection<Tag>(Picture.PictureTags.Select(pt => pt.Tag).ToList());
            People = new ObservableCollection<Person>(Picture.PicturePeople.Select(pp => pp.Person).ToList());

            _cameFromCollection = message.CameFromCollection;
            _sourceCollection = message.Collection;

            DetailEditViewModel = IoC.Get<DetailEditViewModel>();
            EditViewVisibility = Visibility.Collapsed;
            DetailMenuVisibility = Visibility.Visible;
        }

        private async Task ReloadData()
        {
            await _photizerUnitOfWork.PictureRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.TagRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.PersonRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.LenseRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.CameraRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.CategoryRepository.ReloadAll().ConfigureAwait(false);
            await _photizerUnitOfWork.LocationRepository.ReloadAll().ConfigureAwait(false);
        }

        public async Task ToggleEdit()
        {
            if (EditViewVisibility == Visibility.Collapsed)
            {
                EditViewVisibility = Visibility.Visible;
                DetailMenuVisibility = Visibility.Collapsed;
                await _eventAggregator.PublishOnCurrentThreadAsync(new InitDetailEditViewModelMessage() { PictureId = Picture.Id }).ConfigureAwait(false);
            }
            else
            {
                EditViewVisibility = Visibility.Collapsed;
                DetailMenuVisibility = Visibility.Visible;
            }
        }

        public async Task GoBack()
        {
            Image = null;
            Picture = null;
            if (_cameFromCollection)
            {
                await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToCollectionsViewMessage()).ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateToCollectionDetailsMessage()).ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new OpenCollectionDetailsMessage() { Collection = _sourceCollection }).ConfigureAwait(false);
            }
            else
            {
                await _eventAggregator.PublishOnCurrentThreadAsync(new NavigateBackToPictureResultViewMessage()).ConfigureAwait(false);
            }
        }

        public async Task HandleAsync(DetailWasEditedMessage message, CancellationToken cancellationToken)
        {
            _ = await _photizerUnitOfWork.PictureRepository.Reload(Picture).ConfigureAwait(true);
            Picture = await _photizerUnitOfWork.PictureRepository.GetPictureWithAllDataById(message.Picture.Id).ConfigureAwait(false);
            Tags = new ObservableCollection<Tag>(Picture.PictureTags.Select(pt => pt.Tag).ToList());
            People = new ObservableCollection<Person>(Picture.PicturePeople.Select(pp => pp.Person).ToList());
            EditViewVisibility = Visibility.Collapsed;
            DetailMenuVisibility = Visibility.Visible;

            await _eventAggregator.PublishOnCurrentThreadAsync(new ReloadPictureSearchAfterEditMessage() { Picture = Picture }, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task HandleAsync(DetailPageKeywordUpdateMessage message, CancellationToken cancellationToken)
        {
            await ReloadData().ConfigureAwait(false);
        }
    }
}