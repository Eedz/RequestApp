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

        #region Model Properties
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

        [ObservableProperty]
        private Requester selectedRequester;
              
        public ObservableCollection<Requester> Requesters
        {
            get => _model.Requesters;
            set
            {
                if (_model.Requesters != value)
                {
                    _model.Requesters = value;
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

        public bool PartialDataSets
        {
            get => _model.PartialDataSets;
            set
            {
                if (_model.PartialDataSets != value)
                {
                    _model.PartialDataSets = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<ITCDataSet> DataSets => _model.DataSets;
        #endregion
        public List<string> StatusList { get; } = ["Active", "Reminded", "Expired", "Completed"];
        public List<Requester> RequesterNames { get; set; } = [];
        public List<string> AvailableProjects { get; set; } = [];
        public ObservableCollection<ITCDataSet> AvailableDataSets { get; set; } = [];
        public ObservableCollection<ITCDataSet> FilteredDataSets { get; set; } = [];
        public List<string> InternalExternalList { get; } = ["Internal", "External"];

        [ObservableProperty]
        private bool dataFormatSAS;
        [ObservableProperty]
        private bool dataFormatSPSS;
        [ObservableProperty]
        private bool dataFormatSTATA;
        [ObservableProperty]
        private bool dataFormatOther;
        public string DataFormatOtherDescription { get; set; }

        [ObservableProperty]
        private string selectedProject;

        public RequestEditorViewModel(Request model, IDataRequestService requestService)
        {
            _requestService = requestService;
            _original = model;
            _model = model.Clone();
            if (_model.DataFormat.Contains("SAS")) DataFormatSAS = true;
            if (_model.DataFormat.Contains("SPSS")) DataFormatSPSS = true;
            if (_model.DataFormat.Contains("STATA")) DataFormatSTATA = true;
            if (_model.DataFormat.Any(x => x != "SAS" && x != "SPSS" && x != "STATA"))
            {
                DataFormatOther = true;
                DataFormatOtherDescription = _model.DataFormat.FirstOrDefault(x => x != "SAS" && x != "SPSS" && x != "STATA");
            }
        }

        public async Task LoadAsync()
        {
            var requesters = await _requestService.GetRequesters();
            RequesterNames = requesters.OrderBy(x=>x.LastName).ToList();
            OnPropertyChanged(nameof(RequesterNames));
            AvailableProjects = await _requestService.GetProjects();
            OnPropertyChanged(nameof(AvailableProjects));
            AvailableDataSets = new ObservableCollection<ITCDataSet>(await _requestService.GetDataSets());
            FilteredDataSets = new ObservableCollection<ITCDataSet>(AvailableDataSets);
        }

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
        public void AddRequester(Requester requester)
        {
            if (!Requesters.Contains(requester))
            {
                Requesters.Add(requester);
            }            
        }

        [RelayCommand]
        public void RemoveRequester(Requester requester)
        {
            if (Requesters.Contains(requester))
            {
                Requesters.Remove(requester);
            }
        }

        [RelayCommand]
        public void Save()
        {           
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
        public void Cancel()
        {
            OnRequestClose(false);
        }        
    }
}
