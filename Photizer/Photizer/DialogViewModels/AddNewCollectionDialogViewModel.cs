using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewCollectionDialogViewModel : Screen
    {
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
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

        public AddNewCollectionDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(Name))
            {
                Message = Multilang.AddNewCollectionDialog_CollectionNameIsEmpty;
            }
            else
            {
                Collection newCollection = new Collection { Name = Name, Created = DateTime.Now };
                await _photizerUnitOfWork.CollectionRepository.Add(newCollection).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                Message = Multilang.AddNewCollectionDialog_NewCollectionAdded;
                Name = string.Empty;

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