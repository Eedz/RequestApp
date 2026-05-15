using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmLib;
using MvvmLib.ViewModels;
using RequestApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace RequestApp.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IDataRequestService _requestService;
        private readonly IDialogService _dialogService;

        public string RequestFormFolder = @"\\psychfile\psych$\psych-lab-gfong\Personal_Working_Folders\Jamie\Data Access\Approved - Completed Data Request Forms";

        public ObservableCollection<RequestItemViewModel> Requests { get; } = [];
        public int RequestCount => Requests.Count;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFiltered))]
        private ObservableCollection<RequestItemViewModel> displayedRequests;

        public ObservableCollection<Requester> Names { get; set; } = [];
        public ObservableCollection<ITCDataSet> Datasets { get; set; } = [];

        [ObservableProperty]
        private bool showFilterPopup;

        [ObservableProperty]
        private Requester selectedName;
        [ObservableProperty]
        private ITCDataSet selectedDataSet;

        public bool IsFiltered => DisplayedRequests.Count != Requests.Count;

        public List<string> SortOptions { get; } = ["Expiry Date", "Requester"];

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
            List<Request> requests;
            if (true)
            {
                requests = await _requestService.GetRequests();
            }
            else
            {
                var json = await File.ReadAllTextAsync("requests.json");
                requests = JsonSerializer.Deserialize<List<Request>>(json)
                                ?? [];
            }

            // create view models for each request and subscribe to their delete events
            Requests.Clear();
            foreach (var request in requests.OrderByDescending(x=>x.ExpiryDate))
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

            Datasets = new ObservableCollection<ITCDataSet>(await _requestService.GetDataSets());
            
            DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests);
        }

        partial void OnSelectedDataSetChanged(ITCDataSet oldValue, ITCDataSet newValue)
        {
            if (SelectedDataSet != null) 
                SelectedName = null;
        }

        partial void OnSelectedNameChanged(Requester oldValue, Requester newValue)
        {
            if (SelectedName != null)
                SelectedDataSet = null;
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
            var request = new Request
            {
                Status = "New",
                Requester = new Requester()
            };

            var vm = new RequestEditorViewModel(
                request,
                _requestService);

            await vm.LoadAsync();

            var result = _dialogService.ShowDialog(vm);

            if (result == true)
            {
                var success = await _requestService.CreateRequest(vm.Model);

                if (success)
                {
                    Requests.Add(new RequestItemViewModel(
                        vm.Model,
                        _requestService,
                        _dialogService));
                }
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
            if (SelectedName != null)
            {
                DisplayedRequests.Clear();
                DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests.Where(r => r.Requester?.ID == SelectedName.ID));
            }else if (SelectedDataSet != null)
            {
                DisplayedRequests.Clear();
                DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests.Where(r => r.DataSets.Any(x => x.Name == SelectedDataSet.Name)));
            }else
            {
                DisplayedRequests = new ObservableCollection<RequestItemViewModel>(Requests);
            }
            
            
            OnPropertyChanged(nameof(IsFiltered));
            ShowFilterPopup = false;
        }

        [RelayCommand]
        private async Task ClearFilter()
        {
            ShowFilterPopup = false;
            SelectedName = null;
            SelectedDataSet = null;
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

        [RelayCommand]
        private void ViewEmailTemplates()
        {

        }

        [RelayCommand]
        private void UpdateDataSet()
        {
            // show popup for getting which data set
            var vm = new QuickPickerViewModel("Data Set", Datasets);
            if (_dialogService.ShowDialog(vm) != true) return;

            var dataset = (ITCDataSet) vm.SelectedItem;
            // get requests for that data set
            var requests = Requests.Where(x => x.DataSets.Any(d => d.Name == dataset.Name));

            string recipients = string.Join(";", requests.Select(x => x.Requester.Email).Distinct());
            string subject = "Data Set Update";
            string body = datasetUpdateEmail;

            string mailtoUrl = $"mailto:{Uri.EscapeDataString(recipients)}?subject={subject}&body={body}";

            Process.Start(new ProcessStartInfo(mailtoUrl)
            {
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private void GoToRequestFolder()
        {
            Process.Start("explorer.exe", RequestFormFolder);
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        string emailNewUser = @"Hi FIRST NAME, 
Your access to the ITC 4-Country Waves 8-10 data has now expired.  If you have completed your analyses with this data, please let me know so that I can update our data access records and please follow the instructions below for data destruction. If you need to extend your access to this data for the same analyses (for up to another 12 months), please complete and return the signed Data Usage Agreement and Disclosure of Competing Interests forms (attached).   
If you are renewing your data access, please recall these important considerations for analyzing the data:
1)	To better understand the derived variables and the weights variables in the datasets, please read the “Derived Variables Document.pdf” – click File Station -> Released Datasets -> Instructions.
2)	For other data analysis help, please go to File Station -> Statistical Corner -> Data Analysis Help Files.
3)	Data must be interpreted by referencing the most recent version of the ITC surveys.  
•	The variable labels in the datasets, ON THEIR OWN, are not adequate for data interpretation. For various reasons, labels don’t always capture wording differences over waves and countries.
•	The ITC surveys can be downloaded at http://itcproject.org/surveys.  

If you do not need to extend your access:
1)    Please shred the datasets from your computer (per our data policy in line with our ethics requirements). 
2)    Please ensure that the data are completely destroyed by using software that you know will shred the data, such as PermaDelete (https://github.com/encrypt0r/permadelete). A list of other free programs for Windows can be found here: https://www.lifewire.com/free-file-shredder-software-programs-2619149, and a list of free programs for MacOS can be found here: https://news.macgasm.net/reviews/best-free-file-shredders-mac/
3)    Please send a confirmation email to itcdata@uwaterloo.ca once this is done.

If you have any questions, please let me know.
Thanks,
YOUR NAME
";

        string emailRegularRenewal = @"Hi FIRST NAME,
Your access to the ITC Netherlands (Waves 7-8) and United Kingdom (Waves 9-10) for your proposal on socioeconomic differences in impact of smoking cessation medications has expired again, as it has been one year since you last completed the forms for data access. 
If you have now completed your analyses with this data and no longer need the access, please let me know so that I can update our records, and please follow the instructions below for data destruction. If you would like to extend your access for the same analyses (up to another 12 months), please complete and return the signed Data Usage Agreement and Disclosure of Competing Interests forms (attached).
If you do not need to extend your access:
1)  Please shred the datasets from your computer (per our data policy in line with our ethics requirements). 
2)  Please ensure that the data are completely destroyed by using software that you know will shred the data, such as PermaDelete (https://github.com/encrypt0r/permadelete). A list of other free programs for Windows can be found here: https://www.lifewire.com/free-file-shredder-software-programs-2619149, and a list of free programs for MacOS can be found here: https://news.macgasm.net/reviews/best-free-file-shredders-mac/
3)  Please send a confirmation email to me, or to itcdata@uwaterloo.ca once this is done.
 
Thanks,
YOUR NAME
";

        string emailNewUserFollowup = @"Hi FIRST NAME, 
I am following up about your access to the TCP India Wave 1-2 data subset that was sent to you in November. You had indicated that the completion date would be Feb 20, 2019, so if you have now completed your analyses with this data, please let me know so that I can update our records and please follow the instructions below for data destruction.  If you need a bit longer to complete your analyses, please let me know as well and we may be able to just extend your current access for a few more months.   Or, if you need a longer time (up to 12 months), we would ask you to complete the data access renewal process by sending in new forms (attached). 
If you do not need to extend your access:
1)    Please shred the datasets from your computer (per our data policy in line with our ethics requirements). 
2)    Please ensure that the data are completely destroyed by using software that you know will shred the data, such as PermaDelete (https://github.com/encrypt0r/permadelete). A list of other free programs for Windows can be found here: https://www.lifewire.com/free-file-shredder-software-programs-2619149, and a list of free programs for MacOS can be found here: https://news.macgasm.net/reviews/best-free-file-shredders-mac/
3)    Please send a confirmation email to me, or to itcdata@uwaterloo.ca once this is done.

Thanks,
YOUR NAME
";

        string datasetUpdateEmail = @"Hi everyone,
The ITC [DATASET NAME(S)] have just been updated again on the ITC website the Wave Waterloo. This update includes the following changes:

[ADD CHANGES HERE]

Current users of the ITC 4CE data should download the revised datasets by following the instructions below: 

1)	Log in to the website https://thewave.uwaterloo.ca:8443/ 
2)	Go to the [DATASET NAME] data folder: click on ‘File Station’ -> ‘Released Datasets’ -> ‘[PROJECT NAME]’
3)	In that folder you will see 3 sub-folders: SAS, SPSS or Stata. Choose your statistical software format, and go into that folder. 
4)	The data files will be saved as zip files inside these folders. The 4CV1 data files will begin with ‘itc4v’ and include both a ‘core’ file and the main data file (as well as a ‘formats’ file for the SAS version).  To download the files:
a.	Double click on the file,
b.	Right-click on the file and select Download, or
c.	Highlight the file, then go to Actions -> Download
5)	Once you have downloaded the data files, you will need to decrypt them. Open the files in a file decompression software and enter the decryption password when prompted.  
6)	Once you’ve entered the password, the data files should be decrypted, and you should now be able to use them.

Other important considerations for analyzing the data:
1)	To better understand the derived variables in the datasets, please read the “Derived Variables Document.pdf” – click File Station -> Released Datasets -> Instructions.  
2)	To better understand the weights in the datasets, please read the “4CV1 – Sampling Weights” document – click File Station -> Released Datasets -> 4-Country-V.
3)	To better understand the variable names and dataset naming convention, please read the “4CV1 Release Notes” document – click File Station -> Released Datasets -> 4-Country-V. 
4)	For other data analysis help, please go to File Station -> Statistical Corner -> Data Analysis Help Files.
5)	Data must be interpreted by referencing the most recent version of the ITC 4-Country surveys.  
•	The variable labels in the datasets, ON THEIR OWN, are not adequate for data interpretation. For various reasons, labels don’t always capture wording differences over waves and countries.
•	The updated ITC 4CV1 surveys can be downloaded at https://itcproject.org/surveys/.  

Should you have any questions related to the datasets or request a password, please feel free to contact itcdata@uwaterloo.ca.
Thanks,
YOUR NAME
";


    }
}
