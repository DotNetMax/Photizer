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
    public class EditLensesDataViewModel : Screen
    {
        public ICommand OpenAddNewLenseDialogCommand => new DialogCommand(RunAddNewLenseDialog);

        private readonly IEventAggregator _eventAggregator;
        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;

        private ObservableCollection<Lense> _lenses;

        public ObservableCollection<Lense> Lenses
        {
            get { return _lenses; }
            set
            {
                _lenses = value;
                NotifyOfPropertyChange();
            }
        }

        private Lense _selectedLense;

        public Lense SelectedLense
        {
            get { return _selectedLense; }
            set
            {
                _selectedLense = value;
                if (value is null)
                {
                    CanDeleteLense = false;
                }
                else
                {
                    CanDeleteLense = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _canDeleteLense;

        public bool CanDeleteLense
        {
            get { return _canDeleteLense; }
            set
            {
                _canDeleteLense = value;
                NotifyOfPropertyChange();
            }
        }

        public EditLensesDataViewModel(IEventAggregator eventAggregator, IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _eventAggregator = eventAggregator;
            _photizerUnitOfWork = photizerUnitOfWork;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override void OnViewLoaded(object view)
        {
            Task.Run(LoadLenses);

            base.OnViewLoaded(view);
        }

        public async Task RefreshLenses()
        {
            await LoadLenses().ConfigureAwait(false);
        }

        public async Task DeleteLense()
        {
            var askForDeleteResult = MessageBox.Show("Really delete this Lense? " + SelectedLense.Name, "Warning", MessageBoxButton.YesNo);
            if (askForDeleteResult == MessageBoxResult.Yes)
            {
                await _photizerUnitOfWork.LenseRepository.Delete(SelectedLense).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshLenses().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private async Task LoadLenses()
        {
            var lenses = await _photizerUnitOfWork.LenseRepository.GetAll().ConfigureAwait(false);
            lenses = lenses.OrderBy(l => l.Name);
            Lenses = new ObservableCollection<Lense>(lenses);
        }

        private async Task RunAddNewLenseDialog()
        {
            var view = new AddNewLenseDialogView
            {
                DataContext = IoC.Get<AddNewLenseDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await RefreshLenses().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
        }

        public async Task SaveChanges(KeyEventArgs args, Lense lense, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                lense.Name = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshLenses().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}