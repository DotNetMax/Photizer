using Caliburn.Micro;
using Photizer.DialogViewModels;
using Photizer.Domain.Interfaces;
using Photizer.ImageUtilities;
using Photizer.Infrastructure.Data;
using Photizer.Infrastructure.Services;
using Photizer.Infrastructure.Utilities;
using Photizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace Photizer
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.Instance(_container);

            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>()
                .Singleton<SplashScreenViewModel>()
                .PerRequest<PhotizerDbContext>()
                .PerRequest<IPhotizerUnitOfWork, PhotizerUnitOfWork>()
                ;

            _container
                .Singleton<MainMenuViewModel>()
                .Singleton<AddPictureViewModel>()
                .Singleton<AppSettingsViewModel>()
                .Singleton<FolderSettingsViewModel>()
                .Singleton<DatabaseImportExportViewModel>()
                .Singleton<AddNewPersonDialogViewModel>()
                .Singleton<AddNewLocationDialogViewModel>()
                .Singleton<AddNewLenseDialogViewModel>()
                .Singleton<AddNewCameraDialogViewModel>()
                .Singleton<AddNewCategoryDialogViewModel>()
                .Singleton<AddNewTagDialogViewModel>()
                .Singleton<AddNewCollectionDialogViewModel>()
                .Singleton<PictureSearchViewModel>()
                .Singleton<SearchParameterViewModel>()
                .Singleton<SearchResultViewModel>()
                .Singleton<AddPictureSettingsViewModel>()
                .Singleton<PictureDetailViewModel>()
                .Singleton<DetailEditViewModel>()
                .Singleton<CollectionsViewModel>()
                .Singleton<CollectionsOverviewViewModel>()
                .Singleton<CollectionsDetailViewModel>()
                .Singleton<EditDataViewModel>()
                .Singleton<EditLocationDataViewModel>()
                .Singleton<EditCamerasDataViewModel>()
                .Singleton<EditCategoriesDataViewModel>()
                .Singleton<EditLensesDataViewModel>()
                .Singleton<EditPeopleDataViewModel>()
                .Singleton<EditTagsDataViewModel>()
                ;

            _container
                .PerRequest<PictureMapViewModel>()
                .PerRequest<GeocodingDialogViewModel>()
                ;

            _container
                .Singleton<IBitmapImageResizer, BitmapImageResizer>()
                .Singleton<IExifDataExtractor, ExifDataExtractor>()
                .Singleton<IPictureFileManager, PictureFileManager>()
                .Singleton<IAppSettingsService, AppSettingsService>()
                .Singleton<IPhotizerLogger, PhotizerLogger>()
                .PerRequest<IPictureSearcher, PictureSearcher>()
                .Singleton<IDatabaseManager, DatabaseManager>()
                .Singleton<IGeoCodingService, GeoCodingService>()
                ;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<SplashScreenViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            throw e.Exception;
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            var dbManager = IoC.Get<IDatabaseManager>();
            dbManager.ExportDatabase();
            base.OnExit(sender, e);
        }
    }
}