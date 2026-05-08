using CommunityToolkit.Mvvm.Input;
using MvvmLib.ViewModels;
using RequestApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MvvmLib;
using CommunityToolkit.Mvvm.ComponentModel;
namespace RequestApp.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IDataRequestService _requestService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<RequestItemViewModel> Requests { get; } = new ObservableCollection<RequestItemViewModel>();
        public int RequestCount => Requests.Count;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFiltered))]
        private ObservableCollection<RequestItemViewModel> displayedRequests;

        public ObservableCollection<Requester> Names { get; set; } = new ObservableCollection<Requester>();

        [ObservableProperty]
        private bool showFilterPopup;

        [ObservableProperty]
        private Requester selectedName;

        public bool IsFiltered => DisplayedRequests.Count != Requests.Count;

        public List<string> SortOptions { get; } = new List<string>() { "Expiry Date", "Requester" };

        [ObservableProperty]
        private bool showSortPopup;

    [ObservableProperty]
        private string selectedSort;

        public MainViewModel(IDataRequestService requestService, IDialogService dialogService)
        {
            _requestService = requestService;
            _dialogService = dialogService;
        }


        public async Task LoadAsync()
        {
#if DEBUG
            //var json = await File.ReadAllTextAsync("requests.json");
            //var requests = JsonSerializer.Deserialize<List<Request>>(json)
            //               ?? new List<Request>();

            var requests = await _requestService.GetRequests();

#else
            HttpClient _httpClient = new HttpClient();
            var requests = await _httpClient.GetFromJsonAsync<List<Request>>("requests.json");
#endif
            foreach (var request in requests)
            {
                var vm = new RequestItemViewModel(request, _requestService, _dialogService);
                vm.DeleteRequested += Item_DeleteRequested;
                Requests.Add(vm);
            }

            Requests.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(RequestCount));
            };

            Names = new ObservableCollection<Requester>(
                requests.Select(r => r.Requester)
                            .Where(r => r != null)
                            .GroupBy(r => r.ID)
                            .Select(g => g.First())
                            .OrderBy(o => o.LastName)
                            .ToList()
            );
            
            DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests);
        }

        private void Item_DeleteRequested(object sender, System.EventArgs e)
        {
            if (sender is not RequestItemViewModel item)
                return;

            item.DeleteRequested -= Item_DeleteRequested;


            Requests.Remove(item);


        }

        [RelayCommand]
        private async Task Add()
        {
            var newRequest = new Request
            {
                Status = "New",
                Requester = new Requester()
            };
            var newRequestVM = new CreateRequestViewModel(newRequest, _requestService);
            await newRequestVM.Load();
            newRequestVM.SaveRequested += NewItem_SaveRequested;
            var result = _dialogService.ShowDialog(newRequestVM);



        }

        private async void NewItem_SaveRequested(object sender, EventArgs e)
        {
            if (sender is not CreateRequestViewModel createVM)
                return;
            createVM.SaveRequested -= NewItem_SaveRequested;
            var success = await _requestService.CreateRequest(createVM.GetRequest());
            if (success)
            {
                var newItemVM = new RequestItemViewModel(createVM.GetRequest(), _requestService, _dialogService);
                newItemVM.DeleteRequested += Item_DeleteRequested;

                Requests.Add(newItemVM);
                createVM.CloseCommand.Execute(true);
            }
            else
            {
                _dialogService.ShowMessage("Error", "Failed to save the request.");
                //Requests.Remove(createVM);
            }

        }

        [RelayCommand]
        private void ShowFilter()
        {
            
            ShowFilterPopup = true;
        }

        [RelayCommand]
        private void ApplyFilter()
        {
            if (SelectedName == null)
            {
                DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests);
                return;
            }
            DisplayedRequests.Clear();
            DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests.Where(r => r.Requester?.ID == SelectedName.ID));
            OnPropertyChanged(nameof(IsFiltered));
            ShowFilterPopup = false;
        }

        [RelayCommand]
        private async Task ClearFilter()
        {
            ShowFilterPopup = false;
            SelectedName = null;
            ApplyFilter();
        }

        [RelayCommand]
        private void ApplySort()
        {
            switch (SelectedSort)
            {
                case "Expiry Date":
                    DisplayedRequests = new ObservableCollection<RequestItemViewModel>( DisplayedRequests.OrderByDescending(x => x.ExpiryDate));
                    break;
                case "Requester":
                    DisplayedRequests = new ObservableCollection<RequestItemViewModel>(DisplayedRequests.OrderBy(x => x.Requester.LastName));
                    break;
            }
            ShowSortPopup = false;
        }

        [RelayCommand]
        private void ShowSort()
        {
            ShowSortPopup = true;
        }
    }
}
