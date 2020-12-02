using Caliburn.Micro;
using System.Threading.Tasks;

namespace Photizer.ViewModels
{
    public class EditDataViewModel : Conductor<object>
    {
        public EditDataViewModel()
        {
        }

        public async Task NavigateToEditLocations()
        {
            await ActivateItemAsync(IoC.Get<EditLocationDataViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToEditCategories()
        {
            await ActivateItemAsync(IoC.Get<EditCategoriesDataViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToEditTags()
        {
            await ActivateItemAsync(IoC.Get<EditTagsDataViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToEditPeople()
        {
            await ActivateItemAsync(IoC.Get<EditPeopleDataViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToEditCameras()
        {
            await ActivateItemAsync(IoC.Get<EditCamerasDataViewModel>()).ConfigureAwait(false);
        }

        public async Task NavigateToEditLenses()
        {
            await ActivateItemAsync(IoC.Get<EditLensesDataViewModel>()).ConfigureAwait(false);
        }
    }
}