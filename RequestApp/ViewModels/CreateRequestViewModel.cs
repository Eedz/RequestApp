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

        public ObservableCollection<Requester> Requesters { get; set; } = new ObservableCollection<Requester>();
        public ObservableCollection<ITCDataSet> AvailableDataSets { get; set; } = new ObservableCollection<ITCDataSet>();

        public Request GetRequest() => _request;

        [ObservableProperty]
        private bool dropDownNames = false;

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
