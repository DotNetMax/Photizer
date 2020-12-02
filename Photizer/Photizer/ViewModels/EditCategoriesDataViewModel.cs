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
    public class EditCategoriesDataViewModel : Screen
    {
        public ICommand OpenAddNewCategoryDialogCommand => new DialogCommand(RunAddNewCategoryDialog);

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<Category> _categories;

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange();
            }
        }

        private Category _selectedCategory;

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                if (value is null)
                {
                    CanDeleteCategory = false;
                }
                else
                {
                    CanDeleteCategory = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _canDeleteCategory;

        public bool CanDeleteCategory
        {
            get { return _canDeleteCategory; }
            set
            {
                _canDeleteCategory = value;
                NotifyOfPropertyChange();
            }
        }

        public EditCategoriesDataViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override void OnViewLoaded(object view)
        {
            Task.Run(LoadCategories);

            base.OnViewLoaded(view);
        }

        public async Task RefreshCategories()
        {
            await LoadCategories().ConfigureAwait(false);
        }

        private async Task LoadCategories()
        {
            var categories = await _photizerUnitOfWork.CategoryRepository.GetAll().ConfigureAwait(false);
            categories = categories.OrderBy(c => c.Name).ToList();

            Categories = new ObservableCollection<Category>(categories);
        }

        public async Task DeleteCategory()
        {
            var askForDeleteResult = MessageBox.Show("Really delete this Camera? " + SelectedCategory.Name, "Warning", MessageBoxButton.YesNo);
            if (askForDeleteResult == MessageBoxResult.Yes)
            {
                await _photizerUnitOfWork.CategoryRepository.Delete(SelectedCategory).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshCategories().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChanges(KeyEventArgs args, Category category, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                category.Name = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshCategories().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private async Task RunAddNewCategoryDialog()
        {
            var view = new AddNewCategoryDialogView
            {
                DataContext = IoC.Get<AddNewCategoryDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await RefreshCategories().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}