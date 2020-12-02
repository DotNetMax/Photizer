using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using System.Windows.Controls;

namespace Photizer.Views
{
    /// <summary>
    /// Interaction logic for SearchResultView.xaml
    /// </summary>
    public partial class SearchResultView : UserControl
    {
        public SearchResultView()
        {
            InitializeComponent();
        }

        private async void Pictures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            await eventAggregator.PublishOnCurrentThreadAsync(new PictureResultSelectionChangedMessage());
        }
    }
}