using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewCameraDialogViewModel : Screen
    {
        private string _camera;

        public string Camera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                NotifyOfPropertyChange();
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        public AddNewCameraDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(Camera))
            {
                Message = Multilang.AddNewCameraDialog_CameraIsEmpty;
            }
            else
            {
                Camera newCamera = new Camera() { Name = Camera };

                await _photizerUnitOfWork.CameraRepository.Add(newCamera).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);

                Message = Multilang.AddNewCameraDialog_NewCameraAdded;

                Camera = string.Empty;

                await CleanMessage().ConfigureAwait(true);
            }
        }

        private async Task CleanMessage()
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            Message = string.Empty;
        }
    }
}