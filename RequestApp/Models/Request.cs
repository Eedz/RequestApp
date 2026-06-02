using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RequestApp.Models
{
    public class Request
    {
        public int ID { get; set; }
        public string Status { get; set; }
        public ObservableCollection<Requester> Requesters { get; set; } = [];
        public Requester Requester { get; set; }
        public DateTime? LatestSigning { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string AuthorizedBy { get; set; }
        public string InternalExternal { get; set; }
        public string Notes { get; set; }
        public bool PartialDataSets { get; set; }
        public List<string> DataFormat { get; set; } = [];

        public ObservableCollection<ITCDataSet> DataSets { get; set; } = [];


        public Request Clone()
        {
            return new Request
            {
                ID = ID,
                Status = Status,
                Requester = Requester?.Clone(),
                LatestSigning = LatestSigning,
                ExpiryDate = ExpiryDate,
                AuthorizedBy = AuthorizedBy,
                InternalExternal = InternalExternal,
                Notes = Notes,
                PartialDataSets = PartialDataSets,

                // strings are immutable so shallow copy is fine
                DataFormat = new List<string>(DataFormat),

                // clone each dataset
                DataSets = new ObservableCollection<ITCDataSet>(
                    DataSets.Select(ds => ds.Clone())),

                Requesters = new ObservableCollection<Requester>(Requesters.Select(r=>r.Clone()))
            };
        }
    }
}
