using System;
using System.Collections.ObjectModel;

namespace RequestApp.Models
{
    public class Request
    {
        public int ID { get; set; }
        public string Status { get; set; }
        public Requester Requester { get; set; }
        public DateTime? LatestSigning { get; set;  }
        public DateTime? ExpiryDate { get; set;  }
        public string AuthorizedBy { get; set; }
        public string InternalExternal { get; set; }
        public string Notes { get; set; }

        public string Format { get; set; }

        public ObservableCollection<ITCDataSet> DataSets { get; set; } = new();
    }

    
}
