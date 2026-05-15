using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestApp.ViewModels
{
    public partial class QuickPickerViewModel : WorkspaceViewModel
    {
        public string Label { get; }
        public IEnumerable<object> ItemList { get; }
        public object SelectedItem { get; set; }

        public QuickPickerViewModel(string label, IEnumerable<object> items)
        {
            Label = label;
            ItemList = items;
        }

        [RelayCommand]
        private void OK()
        {
            OnRequestClose(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            OnRequestClose(false);
        }
    }
}
