using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmLib.ViewModels;
using RequestApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RequestApp.ViewModels
{
    public partial class CreateRequestViewModel : WorkspaceViewModel
    {
        private readonly IDataRequestService _requestService;

        private readonly Request _request;

        public string Status
        {
            get => _request.Status;
            set
            {
                if (_request.Status != value)
                {
                    _request.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _request.Notes;
            set
            {
                if (_request.Notes != value)
                {
                    _request.Notes = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> DataFormat
        {
            get => _request.DataFormat;
            set
            {
                if (!_request.DataFormat.SequenceEqual(value))
                {
                    _request.DataFormat = value;
                    OnPropertyChanged();
                }
            }
        }

        public Requester Requester
        {
            get => _request.Requester;
            set
            {
                if (_request.Requester != value)
                {
                    _request.Requester = value;
                    OnPropertyChanged();
                    DropDownNames = false;
                }
            }
        }

        public DateTime? LatestSigning
        {
            get => _request.LatestSigning;
            set
            {
                if (_request.LatestSigning != value)
                {
                    _request.LatestSigning = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ExpiryDate
        {
            get => _request.ExpiryDate;
            set
            {
                if (_request.ExpiryDate != value)
                {
                    _request.ExpiryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AuthorizedBy
        {
            get => _request.AuthorizedBy;
            set
            {
                if (_request.AuthorizedBy != value)
                {
                    _request.AuthorizedBy = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ITCDataSet> DataSets
        {
            get => _request.DataSets;
            set
            {
                if (!_request.DataSets.SequenceEqual(value))
                {
                    _request.DataSets = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Requester> Requesters { get; set; } = new ObservableCollection<Requester>();
        public ObservableCollection<ITCDataSet> AvailableDataSets { get; set; } = new ObservableCollection<ITCDataSet>();
        

        public Request GetRequest() => _request;

        [ObservableProperty]
        private bool dropDownNames = false;

        [ObservableProperty]
        private bool dataFormatSAS;
        [ObservableProperty]
        private bool dataFormatSPSS;
        [ObservableProperty]
        private bool dataFormatSTATA;
        [ObservableProperty]
        private bool dataFormatOther;
        public string DataFormatOtherDescription { get; set; }

        public event EventHandler SaveRequested;
        public CreateRequestViewModel(Request request, IDataRequestService dataRequestService) 
        {
            _request = request;
            _requestService = dataRequestService;
        }

        public async Task Load()
        {
            Requesters = new ObservableCollection<Requester>(await _requestService.GetRequesters());
            AvailableDataSets = new ObservableCollection<ITCDataSet>(await _requestService.GetDataSets());

        }

        partial void OnDataFormatSASChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_request.DataFormat.Contains("SAS"))
                    _request.DataFormat.Add("SAS");
            }
            else
            {
                if (_request.DataFormat.Contains("SAS"))
                    _request.DataFormat.Remove("SAS");
            }
        }

        partial void OnDataFormatSPSSChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_request.DataFormat.Contains("SPSS"))
                    _request.DataFormat.Add("SPSS");
            }
            else
            {
                if (_request.DataFormat.Contains("SPSS"))
                    _request.DataFormat.Remove("SPSS");
            }
        }

        partial void OnDataFormatSTATAChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_request.DataFormat.Contains("STATA"))
                    _request.DataFormat.Add("STATA");
            }
            else
            {
                if (_request.DataFormat.Contains("STATA"))
                    _request.DataFormat.Remove("STATA");
            }
        }

        partial void OnDataFormatOtherChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (!_request.DataFormat.Any(x=>x != "SAS" && x != "SPSS" && x != "STATA"))
                    _request.DataFormat.Add(DataFormatOtherDescription);
            }
            else
            {
                if (_request.DataFormat.Any(x => x != "SAS" && x != "SPSS" && x != "STATA"))
                    _request.DataFormat.RemoveAll(x => x != "SAS" && x != "SPSS" && x != "STATA");
            }
        }

        [RelayCommand]
        private void Save()
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            OnRequestClose(false);
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
        private void ShowNames()
        {
            DropDownNames = !DropDownNames;
        }
    }
}
