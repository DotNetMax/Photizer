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
    public class EditPeopleDataViewModel : Screen
    {
        public ICommand OpenAddNewPersonDialogCommand => new DialogCommand(RunAddNewPersonDialog);

        private readonly IPhotizerUnitOfWork _photizerUnitOfWork;
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<Person> _people;

        public ObservableCollection<Person> People
        {
            get { return _people; }
            set
            {
                _people = value;
                NotifyOfPropertyChange();
            }
        }

        private Person _selectedPeople;

        public Person SelectedPeople
        {
            get { return _selectedPeople; }
            set
            {
                _selectedPeople = value;
                if (value is null)
                {
                    CanDeletePerson = false;
                }
                else
                {
                    CanDeletePerson = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool _canDeletePerson;

        public bool CanDeletePerson
        {
            get { return _canDeletePerson; }
            set
            {
                _canDeletePerson = value;
                NotifyOfPropertyChange();
            }
        }

        public EditPeopleDataViewModel(IPhotizerUnitOfWork photizerUnitOfWork, IEventAggregator eventAggregator)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected override void OnViewLoaded(object view)
        {
            Task.Run(LoadPeople);

            base.OnViewLoaded(view);
        }

        public async Task RefreshPeople()
        {
            await LoadPeople().ConfigureAwait(false);
        }

        public async Task SaveChangesFirstName(KeyEventArgs args, Person person, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                person.FirstName = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshPeople().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        public async Task SaveChangesLastName(KeyEventArgs args, Person person, TextBox source)
        {
            if (args != null && args.Key == Key.Enter)
            {
                person.LastName = source.Text;
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshPeople().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private async Task LoadPeople()
        {
            var people = await _photizerUnitOfWork.PersonRepository.GetAll().ConfigureAwait(false);
            people = people.OrderBy(p => p.LastName).ToList();
            People = new ObservableCollection<Person>(people);
        }

        private async Task RunAddNewPersonDialog()
        {
            var view = new AddNewPersonDialogView
            {
                DataContext = IoC.Get<AddNewPersonDialogViewModel>()
            };
            await DialogHost.Show(view, "RootDialog", ClosingEventHandler).ConfigureAwait(true);
            await RefreshPeople().ConfigureAwait(false);
            await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
        }

        public async Task DeletePerson()
        {
            var askForDeleteResult = MessageBox.Show("Really delete this Person? " + SelectedPeople.FirstName + " " + SelectedPeople.LastName, "Warning", MessageBoxButton.YesNo);
            if (askForDeleteResult == MessageBoxResult.Yes)
            {
                await _photizerUnitOfWork.PersonRepository.Delete(SelectedPeople).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                await RefreshPeople().ConfigureAwait(false);
                await _eventAggregator.PublishOnCurrentThreadAsync(new NotifyKeywordDataChangedMessage()).ConfigureAwait(false);
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
        }
    }
}