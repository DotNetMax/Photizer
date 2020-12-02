using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewTagDialogViewModel : Screen
    {
        private string _tag;

        public string Tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
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

        public AddNewTagDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(Tag))
            {
                Message = Multilang.AddNewTagDialog_TagIsEmpty;
            }
            else
            {
                Tag newTag = new Tag() { Name = Tag };

                await _photizerUnitOfWork.TagRepository.Add(newTag).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);

                Message = Multilang.AddNewTagDialog_NewTagAdded;

                Tag = string.Empty;

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