using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRI.Models
{
    public class SubscriberInformation : Member
    {
        public string MaritalStatus { get; set; }
        public string MailingAddressLine1 { get; set; }
        public string MailingAddressLine2 { get; set; }
        public string MailingAddressLine3 { get; set; }
        public string MailingCity { get; set; }
        public string MailingState { get; set; }
        public string MailingZipCode { get; set; }
        public string PhysicalAddressLine1 { get; set; }
        public string PhysicalAddressLine2 { get; set; }
        public string PhysicalAddressLine3 { get; set; }
        public string PhysicalCity { get; set; }
        public string PhysicalState { get; set; }
        public string PhysicalZipCode { get; set; }
        public string PhysicalCounty { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string EmailAddress { get; set; }
        public string CommunicationPreference { get; set; }
        public string PlanName { get; set; }
        public string GroupName { get; set; }
        public string Market { get; set; }
        public string Status { get; set; }
        public decimal? PremiumAmount { get; set; }
        public DateTime EnrolledOn { get; set; }
        public DateTime LastChange { get; set; }
        public string Relationship { get; set; }
        public List<DependentInformation> Dependents { get; set; }
    }
}