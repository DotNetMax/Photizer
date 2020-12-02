using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Photizer.Commands;
using Photizer.DialogViewModels;
using Photizer.DialogViews;
using Photizer.Domain.Entities;
using Photizer.Domain.EventMessages;
using Photizer.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Photizer.ViewModels
{
    public class EditCamerasDataViewModel : Screen
    {
        public ICommand OpenAddNewCameraDialogCommand => new DialogCommand(RunAddNewCameraDialog);

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<Camera> _cameras;

        public ObservableCollection<Camera> Cameras
        {
            get { return _cameras; }
            set
            {
                _cameras = value;
                NotifyOfPropertyChange();
            }
        }

        private Camera _selectedCamera;

        public Camera SelectedCamera
        {
            get { return _selectedCamera; }
            set
            {
                _selectedCamera = value;
                if (value is null)
                {
                    CanDeleteCamera = false;
                }
                else
                {
                    CanDeleteCamera = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _canDeleteCamera;

        public bool CanDeleteCamera
        {
            get { return _canDeleteCamera; }
            set
            {
                _canDeleteCamera = value;
                NotifyOfPropertyChange();
            }
        }

        public EditCamerasDataViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override void OnViewLoaded(object view)
        {
            Task.Run(() => LoadCameras());

            base.OnViewLoaded(view);
        }

        public async Task RefreshCameras()
        {
            await LoadCameras().ConfigureAwait(false);
        }

        private async Task LoadCameras()
        {
            var cameras = await _photizerUnitOfWork.CameraRepository.GetAll().ConfigureAwait(false);
            cameras = cameras.OrderBy(c => c.Name).ToList();
            Cameras = new ObservableCollection<Camera>(cameras);
        }

        public async Task DeleteCamera()
        {
            var askForDeleteResult = MessageBox.Show("Really delete this Camera? " + SelectedCamera.Name, "Warning", MessageBoxButton.YesNo);
            if (askForDeleteResult == MessageBoxResult.Yes)
            {
                await _photizerUnitOfWork.CameraRepository.Delete(SelectedCamera).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshCameras().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChanges(KeyEventArgs args, Camera camera, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                camera.Name = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshCameras().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private async Task RunAddNewCameraDialog()
        {
            var view = new AddNewCameraDialogView
            {
                DataContext = IoC.Get<AddNewCameraDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await RefreshCameras().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}