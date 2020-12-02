using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using Photizer.Domain.Entities;
using Photizer.Domain.EventMessages;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Photizer.Views
{
    /// <summary>
    /// Interaction logic for AddPictureView.xaml
    /// </summary>
    public partial class AddPictureView : UserControl
    {
        public AddPictureView()
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

        private void Categories_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var eventAggregator = IoC.Get<IEventAggregator>();
                var comboBox = (sender as ComboBox);
                string typedName = comboBox.Text;

                Task.Run(() => eventAggregator.PublishOnCurrentThreadAsync(new KeyDownEventMessage<Category> { Content = typedName }));
            }
        }

        private void Cameras_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var eventAggregator = IoC.Get<IEventAggregator>();
                var comboBox = (sender as ComboBox);
                string typedName = comboBox.Text;

                Task.Run(() => eventAggregator.PublishOnCurrentThreadAsync(new KeyDownEventMessage<Camera> { Content = typedName }));
            }
        }

        private void Lenses_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var eventAggregator = IoC.Get<IEventAggregator>();
                var comboBox = (sender as ComboBox);
                string typedName = comboBox.Text;

                Task.Run(() => eventAggregator.PublishOnCurrentThreadAsync(new KeyDownEventMessage<Lense> { Content = typedName }));
            }
        }

        private void Tags_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var eventAggregator = IoC.Get<IEventAggregator>();
                var comboBox = (sender as ComboBox);
                string typedName = comboBox.Text;

                Task.Run(() => eventAggregator.PublishOnCurrentThreadAsync(new KeyDownEventMessage<Tag> { Content = typedName }));
            }
        }
    }
}