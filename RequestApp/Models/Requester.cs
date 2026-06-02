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
        public DateTime? LatestSigning { get; set; }
        public string CountryTeamMember { get; set; }
        public string WebsiteMember { get; set; }
        public bool DataUser { get; set; }
        public bool StaffMember { get; set;  }
        public bool CoreMember { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string ReversedFullName => $"{LastName}, {FirstName}";

        public Requester Clone()
        {
            return new Requester
            {
                ID = ID,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Affiliation = Affiliation,
                CountryTeamMember = CountryTeamMember,
                WebsiteMember = WebsiteMember,
                DataUser = DataUser,
                LatestSigning = LatestSigning,
            };
        }
    }
}
