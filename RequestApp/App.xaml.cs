using RequestApp.ViewModels;
using System;
using System.Configuration;
using System.Windows;

namespace RequestApp
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            MainWindow window = new MainWindow();
            string connectionString = ConfigurationManager.ConnectionStrings["ISISConnectionString"].ConnectionString;
            var repo = new DataRequestRepository(connectionString);
            var requestService = new RepoDataRequestService(repo);
            var dialogService = new DialogService();
            var viewModel = new MainViewModel(requestService, dialogService);
            await viewModel.LoadAsync();

            window.DataContext = viewModel;

            window.Show();
        }
    }
}
