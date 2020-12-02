using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewCategoryDialogViewModel : Screen
    {
        private string _category;

        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
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

        public AddNewCategoryDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(Category))
            {
                Message = Multilang.AddNewCategoryDialog_CategoryIsEmpty;
            }
            else
            {
                Category newCategory = new Category() { Name = Category };

                await _photizerUnitOfWork.CategoryRepository.Add(newCategory).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);

                Message = Multilang.AddNewCategoryDialog_NewCategoryAdded;

                Category = string.Empty;

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