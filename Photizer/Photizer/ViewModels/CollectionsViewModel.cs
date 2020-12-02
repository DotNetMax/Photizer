using Caliburn.Micro;
using Photizer.Domain.EventMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Photizer.ViewModels
{
    public class CollectionsViewModel : Conductor<object>, IHandle<NavigateToCollectionDetailsMessage>, IHandle<NavigateBackToCollectionsMessage>
    {
        private readonly IEventAggregator _eventAggregator;

        public CollectionsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        protected async override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await ActivateItemAsync(IoC.Get<CollectionsOverviewViewModel>(), cancellationToken).ConfigureAwait(false);
        }

        private async Task NavigateToCollectionsOverviewView()
        {
            await ActivateItemAsync(IoC.Get<CollectionsOverviewViewModel>()).ConfigureAwait(false);
        }

        private async Task NavigateToCollectionsDetailView()
        {
            await ActivateItemAsync(IoC.Get<CollectionsDetailViewModel>()).ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateToCollectionDetailsMessage message, CancellationToken cancellationToken)
        {
            await NavigateToCollectionsDetailView().ConfigureAwait(false);
        }

        public async Task HandleAsync(NavigateBackToCollectionsMessage message, CancellationToken cancellationToken)
        {
            await NavigateToCollectionsOverviewView().ConfigureAwait(false);
        }
    }
}