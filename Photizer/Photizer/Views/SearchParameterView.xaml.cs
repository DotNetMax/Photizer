using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Photizer.Domain.EventMessages;
using System.Windows;
using System.Windows.Controls;

namespace Photizer.Views
{
    /// <summary>
    /// Interaction logic for SearchParameterView.xaml
    /// </summary>
    public partial class SearchParameterView : UserControl
    {
        public SearchParameterView()
        {
            InitializeComponent();
        }

        private async void Tag_DeleteClick(object sender, RoutedEventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            var tagChip = (Chip)sender;
            RemoveTagMessage message = new RemoveTagMessage
            {
                TagName = tagChip.Content.ToString()
            };
            await eventAggregator.PublishOnCurrentThreadAsync(message);
        }

        private async void Person_DeleteClick(object sender, RoutedEventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            var tagChip = (Chip)sender;
            RemovePersonMessage message = new RemovePersonMessage
            {
                FullName = tagChip.Content.ToString()
            };
            await eventAggregator.PublishOnCurrentThreadAsync(message);
        }

        private void RatingBar_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RatingBar.Value = 0;
        }
    }
}