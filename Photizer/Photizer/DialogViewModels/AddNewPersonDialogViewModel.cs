using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewPersonDialogViewModel : Screen
    {
        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                NotifyOfPropertyChange(() => FirstName);
            }
        }

        private string _lastName;

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                NotifyOfPropertyChange(() => LastName);
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        public AddNewPersonDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(FirstName))
            {
                Message = Multilang.AddNewPersonDialog_FirstnameIsEmpty;
            }
            else if (string.IsNullOrEmpty(LastName))
            {
                Message = Multilang.AddNewPersonDialog_LastnameIsEmpty;
            }
            else
            {
                Person newPerson = new Person { FirstName = FirstName, LastName = LastName };
                await _photizerUnitOfWork.PersonRepository.Add(newPerson).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                Message = Multilang.AddNewPersonDialog_NewPersonAdded;
                FirstName = string.Empty;
                LastName = string.Empty;

                await CleanMessage().ConfigureAwait(true);
            }
        }

        private async Task CleanMessage()
        {
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            Message = string.Empty;
        }
    }
}