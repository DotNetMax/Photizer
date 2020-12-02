using Caliburn.Micro;
using Photizer.Domain.Entities;
using Photizer.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Photizer.DialogViewModels
{
    public class AddNewLocationDialogViewModel : Screen
    {
        private string _country;

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                NotifyOfPropertyChange(() => Country);
            }
        }

        private string _place;

        public string Place
        {
            get { return _place; }
            set
            {
                _place = value;
                NotifyOfPropertyChange(() => Place);
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

        public AddNewLocationDialogViewModel(IPhotizerUnitOfWork photizerUnitOfWork)
        {
            _photizerUnitOfWork = photizerUnitOfWork;
        }

        public async Task AddData()
        {
            if (string.IsNullOrEmpty(Country))
            {
                Message = Multilang.AddNewLocationDialog_CountryIsEmpty;
            }
            else if (string.IsNullOrEmpty(Place))
            {
                Message = Multilang.AddNewLocationDialog_PlaceIsEmpty;
            }
            else
            {
                Location newLocation = new Location() { Country = Country, Place = Place, Latitude = 0, Longitude = 0 };

                await _photizerUnitOfWork.LocationRepository.Add(newLocation).ConfigureAwait(false);
                await _photizerUnitOfWork.Save().ConfigureAwait(false);
                Message = Multilang.AddNewLocationDialog_NewLocationAdded;
                Country = string.Empty;
                Place = string.Empty;

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