using MvvmLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RequestApp
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message, string title = "Info")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool Confirm(string message, string title = "Confirm")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        public string PromptForText(string message, string title = "Input")
        {
            return null;
        }

        
        public string OpenFile(string filter)
        {
            throw new NotImplementedException();
        }

        public string OpenSurveyImageFile()
        {
            throw new NotImplementedException();
        }

        

        public bool? ShowDialog(WorkspaceViewModel viewModel)
        {
            var window = new Window
            {
                Title = viewModel.DisplayName ?? string.Empty,
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = new ContentControl { Content = viewModel },
                DataContext = viewModel,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner

            };

            // Keep a strong reference to the handler so we can unsubscribe
            EventHandler<DialogResultEventArgs>? handler = null;

            handler = (s, args) =>
            {
                // Detach to break the ViewModel → Window reference chain
                viewModel.RequestClose -= handler;

                // If you use a custom EventArgs with a DialogResult
                window.DialogResult = args.DialogResult;
                window.Close();
            };

            viewModel.RequestClose += handler;

            // Clean up after the dialog closes (no matter how it closes)
            window.Closed += (s, e) =>
            {
                // Dispose if the VM implements IDisposable
                if (viewModel is IDisposable disposable)
                    disposable.Dispose();
            };

            return window.ShowDialog();
        }

        public Task<bool?> ShowDialogAsync<TViewModel>(Func<TViewModel, Task> configure = null) where TViewModel : WorkspaceViewModel
        {
            throw new NotImplementedException();
        }

      

        public void ShowWindow(WorkspaceViewModel viewModel)
        {
            throw new NotImplementedException();
        }
    }
}
