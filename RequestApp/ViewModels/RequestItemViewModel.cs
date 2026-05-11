using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Vml.Office;
using ITCLib;
using MvvmLib;
using MvvmLib.ViewModels;
using RequestApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace RequestApp.ViewModels
{
    public partial class RequestItemViewModel : ViewModelBase
    {
        private readonly IDataRequestService _requestService;
        private readonly IDialogService _dialogService;

        private Request _model;

        public Request Model { get => _model; private set { 
                _model = value; 
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Requester));
                OnPropertyChanged(nameof(LatestSigning));
                OnPropertyChanged(nameof(ExpiryDate));
                OnPropertyChanged(nameof(AuthorizedBy));
                OnPropertyChanged(nameof(InternalExternal));
                OnPropertyChanged(nameof(Notes));
                OnPropertyChanged(nameof(DataSets));
                OnPropertyChanged(nameof(DataSetCount));
            } }
        public RequestEditorViewModel EditorViewModel { get; set; }
        [ObservableProperty]
        private bool isEditing = false;

        public bool IsNew => _model.ID == 0;

        #region Model properties
        public string Status => _model.Status;
        public Requester Requester => _model.Requester;
        public DateTime? LatestSigning => _model.LatestSigning;
        public DateTime? ExpiryDate => _model.ExpiryDate;
        public string AuthorizedBy => _model.AuthorizedBy;
        public string InternalExternal => _model.InternalExternal;
        public string Notes => _model.Notes;
        public string DataFormat => string.Join(", ", _model.DataFormat);
        public ObservableCollection<ITCDataSet> DataSets => _model.DataSets;
        public int DataSetCount => _model.DataSets.Count;   

        #endregion 

        /// <summary>
        /// Raised when this item has been successfully deleted, so the parent list can remove it.
        /// </summary>
        public event EventHandler? DeleteRequested;

        public RequestItemViewModel(Request model, IDataRequestService requestService, IDialogService dialogService) 
        { 
            _model = model;
            _requestService = requestService;
            _dialogService = dialogService; 
            
        }


        

        [RelayCommand]
        private void Email()
        {
            string subject = WebUtility.UrlEncode("Data Request Reminder");
            string body = WebUtility.UrlEncode("Line 1\nLine 2");

            string mailto = $"mailto:test@example.com?subject={subject}&body={body}";

           

            Process.Start(new ProcessStartInfo(mailto)
            {
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private async Task Edit()
        {
            IsEditing = true;

            
            
            EditorViewModel = new RequestEditorViewModel(_model, _requestService);
            await EditorViewModel.LoadAsync();
            EditorViewModel.RequestClose += (s, e) => { 
                IsEditing = false;
                if (e.DialogResult == true)
                    Model = EditorViewModel.Model; // Update the model with any changes from the editor
                
            };
            OnPropertyChanged(nameof(EditorViewModel));

            

            
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (IsNew)
            {
                if (_dialogService.Confirm("Are you sure you want to cancel this request?", "Confirm Delete"))
                    
                    DeleteRequested?.Invoke(this, EventArgs.Empty);

                return;
            }
            if (_dialogService.Confirm("Are you sure you want to delete this request?", "Confirm Delete"))
                if (_dialogService.Confirm("Just to be extra sure. Are you sure you want to delete it?", "Confirm Delete"))
                    if (await _requestService.DeleteRequest(_model.ID))
                        DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
