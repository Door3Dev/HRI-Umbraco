using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class Member
    {
        public string MemberId { get; set; }
        public string FamilyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public string SubscriberFlag { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
    }
}