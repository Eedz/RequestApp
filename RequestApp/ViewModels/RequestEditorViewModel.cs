using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using ITCLib;
using MvvmLib.ViewModels;
using RequestApp.Models;

namespace RequestApp.ViewModels
{
    public partial class RequestEditorViewModel : WorkspaceViewModel 
    {
        private readonly IDataRequestService _requestService;
        private readonly Request _model;

        public List<Requester> RequesterNames { get; set; } = new List<Requester>();
        public List<string> AvailableProjects { get; set; } = new List<string>();
        public ObservableCollection<ITCDataSet> AvailableDataSets { get; set; } = new ObservableCollection<ITCDataSet>();
        public ObservableCollection<ITCDataSet> FilteredDataSets { get; set; } = new ObservableCollection<ITCDataSet>();


        [ObservableProperty]
        private bool dropDownNames = false;

        [ObservableProperty]
        private string selectedProject;

        public RequestEditorViewModel(Request model, IDataRequestService requestService)
        {
            _requestService = requestService;
            
            _model = model;
        }

        public async Task LoadAsync()
        {
            RequesterNames = await _requestService.GetRequesters();
            OnPropertyChanged(nameof(RequesterNames));
            AvailableProjects = await _requestService.GetProjects();
            OnPropertyChanged(nameof(AvailableProjects));
            AvailableDataSets = new ObservableCollection<ITCDataSet>(await _requestService.GetDataSets());
            FilteredDataSets = new ObservableCollection<ITCDataSet>(AvailableDataSets);
        }

        public string Status
        {
            get => _model.Status;
            set
            {
                if (_model.Status != value)
                {
                    _model.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public Requester Requester
        {
            get => _model.Requester;
            set
            {
                if (_model.Requester != value)
                {
                    _model.Requester = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? LatestSigning
        {
            get => _model.LatestSigning;
            set
            {
                if (_model.LatestSigning != value)
                {
                    _model.LatestSigning = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ExpiryDate
        {
            get => _model.ExpiryDate;
            set
            {
                if (_model.ExpiryDate != value)
                {
                    _model.ExpiryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AuthorizedBy
        {
            get => _model.AuthorizedBy;
            set
            {
                if (_model.AuthorizedBy != value)
                {
                    _model.AuthorizedBy = value;
                    OnPropertyChanged();
                }
            }
        }

        public string InternalExternal
        {
            get => _model.InternalExternal;
            set
            {
                if (_model.InternalExternal != value)
                {
                    _model.InternalExternal = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _model.Notes;
            set
            {
                if (_model.Notes != value)
                {
                    _model.Notes = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ITCDataSet> DataSets => new ObservableCollection<ITCDataSet>(_model.DataSets);

        partial void OnSelectedProjectChanged(string oldValue, string newValue)
        {
            if (newValue == "<All>")
                FilteredDataSets = new ObservableCollection<ITCDataSet>(AvailableDataSets);
            else
                FilteredDataSets = new ObservableCollection<ITCDataSet>(AvailableDataSets.Where(ds => ds.ProjectName == newValue));

            OnPropertyChanged(nameof(FilteredDataSets));
            
        }

        [RelayCommand]
        public void Save()
        {
            // Implement save logic, e.g., send updated request to server or update local collection
            if (_model.ID == 0)
                _requestService.CreateRequest(_model);
            else
                _requestService.UpdateRequest(_model);

            OnRequestClose(true);
        }

        [RelayCommand]
        public void ShowNames()
        {
            DropDownNames = !DropDownNames;
            
        }

        [RelayCommand]
        public void Cancel()
        {
            OnRequestClose(false);
        }

        
    }
}
