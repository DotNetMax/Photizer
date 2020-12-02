using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Photizer.Domain.EventMessages;
using System.Windows;
using System.Windows.Controls;

namespace Photizer.Views
{
    /// <summary>
    /// Interaction logic for DetailEditView.xaml
    /// </summary>
    public partial class DetailEditView : UserControl
    {
        public DetailEditView()
        {
            InitializeComponent();
        }

        private async void Tag_DeleteClick(object sender, RoutedEventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            var tagChip = (Chip)sender;
            RemoveDetailEditTagMessage message = new RemoveDetailEditTagMessage
            {
                TagName = tagChip.Content.ToString()
            };
            await eventAggregator.PublishOnCurrentThreadAsync(message);
        }

        private async void Person_DeleteClick(object sender, RoutedEventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            var tagChip = (Chip)sender;
            RemoveDetailEditPersonMessage message = new RemoveDetailEditPersonMessage
            {
                FullName = tagChip.Content.ToString()
            };
            await eventAggregator.PublishOnCurrentThreadAsync(message);
        }
    }
}