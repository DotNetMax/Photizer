using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewLenseDialogViewModel : Screen
    {
        private string _lense;

        public string Lense
        {
            get { return _lense; }
            set
            {
                _lense = value;
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

        public AddNewLenseDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(Lense))
            {
                Message = Multilang.AddNewLenseDialog_LenseIsEmpty;
            }
            else
            {
                Lense newLense = new Lense() { Name = Lense };

                await _photizerUnitOfWork.LenseRepository.Add(newLense).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);

                Message = Multilang.AddNewLenseDialog_NewLenseAdded;

                Lense = string.Empty;

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