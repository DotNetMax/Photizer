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
    public class EditTagsDataViewModel : Screen
    {
        public ICommand OpenAddNewTagDialogCommand => new DialogCommand(RunAddNewTagDialog);

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;

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

        private Tag _selectedTag;

        public Tag SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                _selectedTag = value;
                if (value is null)
                {
                    CanDeleteTag = false;
                }
                else
                {
                    CanDeleteTag = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _canDeleteTag;

        public bool CanDeleteTag
        {
            get { return _canDeleteTag; }
            set
            {
                _canDeleteTag = value;
                NotifyOfPropertyChange();
            }
        }

        public EditTagsDataViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override void OnViewLoaded(object view)
        {
            Task.Run(() => LoadTags());

            CanDeleteTag = false;

            base.OnViewLoaded(view);
        }

        public async Task RefreshTags()
        {
            await LoadTags().ConfigureAwait(false);
        }

        public async Task DeleteTag()
        {
            var askForDeleteResult = MessageBox.Show("Really delete this Tag? " + SelectedTag.Name, "Warning", MessageBoxButton.YesNo);
            if (askForDeleteResult == MessageBoxResult.Yes)
            {
                await _photizerUnitOfWork.TagRepository.Delete(SelectedTag).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshTags().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChanges(KeyEventArgs args, Tag tag, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                tag.Name = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshTags().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private async Task LoadTags()
        {
            var tags = await _photizerUnitOfWork.TagRepository.GetAll().ConfigureAwait(false);
            tags = tags.OrderBy(t => t.Name).ToList();
            Tags = new ObservableCollection<Tag>(tags);
        }

        private async Task RunAddNewTagDialog()
        {
            var view = new AddNewTagDialogView
            {
                DataContext = IoC.Get<AddNewTagDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await RefreshTags().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}