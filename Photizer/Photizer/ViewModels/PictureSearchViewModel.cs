using Caliburn.Micro;

namespace Photizer.ViewModels
{
    public class PictureSearchViewModel : Screen
    {
        private SearchParameterViewModel _searchParameterViewModel;
        private SearchResultViewModel _searchResultViewModel;

        public SearchResultViewModel SearchResultViewModel
        {
            get { return _searchResultViewModel; }
            set
            {
                _searchResultViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public SearchParameterViewModel SearchParameterViewModel
        {
            get { return _searchParameterViewModel; }
            set
            {
                _searchParameterViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public PictureSearchViewModel()
        {
        }

        protected override void OnViewLoaded(object view)
        {
            SearchResultViewModel = IoC.Get<SearchResultViewModel>();
            SearchParameterViewModel = IoC.Get<SearchParameterViewModel>();
        }
    }
}