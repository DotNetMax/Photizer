using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using System.Windows.Controls;

namespace Photizer.Views
{
    /// <summary>
    /// Interaction logic for CollectionsDetailView.xaml
    /// </summary>
    public partial class CollectionsDetailView : UserControl
    {
        public CollectionsDetailView()
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