using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestApp.Models
{
    public class Requester
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Affiliation { get; set; }
        public string Email { get; set; }

        public string CountryTeamMember { get; set; }
        public string WebsiteMember { get; set; }
        public bool DataUser { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string ReversedFullName => $"{LastName}, {FirstName}";

    }
}
