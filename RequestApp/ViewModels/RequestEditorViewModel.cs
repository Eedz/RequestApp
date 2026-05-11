using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using RequestApp.Models;

namespace RequestApp.ViewModels
{
    public partial class RequestEditorViewModel : WorkspaceViewModel 
    {
        private readonly IDataRequestService _requestService;
        private readonly Request _original;
        private readonly Request _model;

        public Request Model => _model;

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
            _original = model;
            _model = new Request()
            {
                ID = model.ID,
                Status = model.Status,
                Requester = model.Requester,    
                LatestSigning = model.LatestSigning,
                ExpiryDate = model.ExpiryDate,
                AuthorizedBy = model.AuthorizedBy,
                InternalExternal = model.InternalExternal,
                Notes = model.Notes,
                DataSets = new ObservableCollection<ITCDataSet>(model.DataSets)
            };
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

        [ObservableProperty]
        private bool dataFormatSAS;
        [ObservableProperty]
        private bool dataFormatSPSS;
        [ObservableProperty]
        private bool dataFormatSTATA;
        [ObservableProperty]
        private bool dataFormatOther;
        public string DataFormatOtherDescription { get; set; }

        public ObservableCollection<ITCDataSet> DataSets => _model.DataSets;

        partial void OnSelectedProjectChanged(string oldValue, string newValue)
        {
            if (newValue == "<All>")
                FilteredDataSets = new ObservableCollection<ITCDataSet>(AvailableDataSets);
            else
                FilteredDataSets = new ObservableCollection<ITCDataSet>(AvailableDataSets.Where(ds => ds.ProjectName == newValue));

            OnPropertyChanged(nameof(FilteredDataSets));
            
        }

        partial void OnDataFormatSASChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_model.DataFormat.Contains("SAS"))
                    _model.DataFormat.Add("SAS");
            }
            else
            {
                if (_model.DataFormat.Contains("SAS"))
                    _model.DataFormat.Remove("SAS");
            }
        }

        partial void OnDataFormatSPSSChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_model.DataFormat.Contains("SPSS"))
                    _model.DataFormat.Add("SPSS");
            }
            else
            {
                if (_model.DataFormat.Contains("SPSS"))
                    _model.DataFormat.Remove("SPSS");
            }
        }

        partial void OnDataFormatSTATAChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_model.DataFormat.Contains("STATA"))
                    _model.DataFormat.Add("STATA");
            }
            else
            {
                if (_model.DataFormat.Contains("STATA"))
                    _model.DataFormat.Remove("STATA");
            }
        }

        partial void OnDataFormatOtherChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_model.DataFormat.Any(x => x != "SAS" && x != "SPSS" && x != "STATA"))
                    _model.DataFormat.Add(DataFormatOtherDescription);
            }
            else
            {
                if (_model.DataFormat.Any(x => x != "SAS" && x != "SPSS" && x != "STATA"))
                    _model.DataFormat.RemoveAll(x => x != "SAS" && x != "SPSS" && x != "STATA");
            }
        }

        [RelayCommand]
        public void Save()
        {           
            _requestService.UpdateRequest(_model);
            OnRequestClose(true);
        }

        [RelayCommand]
        private void AddDataSet(ITCDataSet dataset)
        {
            DataSets.Add(dataset);
        }

        [RelayCommand]
        private void RemoveDataSet(ITCDataSet dataset)
        {
            DataSets.Remove(dataset);
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
