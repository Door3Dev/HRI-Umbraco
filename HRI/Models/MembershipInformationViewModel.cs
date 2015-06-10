using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace HRI.Models
{
    public class MembershipInformationViewModel
    {

        [Display(Name = "Member Id: ")]
        public string MemberId { get; set; }

        [Display(Name="Name:")]
        public string MemberName { get; set; }

        [Display(Name = "SSN: ")]
        public string SSN { get; set; }

        [Display(Name = "Enrolled As: ")]
        public string EnrolledAs { get; set; }

        public string SubscriberFlag { get; set; }

        [Display(Name = "Gender: ")]
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth: ")]
        public DateTime DOB { get; set; }

        [Display(Name = "Marital Status: ")]
        public string MaritalStatus { get; set; }

        [Display(Name = "Relationship: ")]
        public string Relationship { get; set; }

        [Display(Name = "Address 1: ")]
        public String MailingAddressLine1 { get; set; }

        [Display(Name = "Address 2:")]
        public String MailingAddressLine2 { get; set; }

        [Display(Name = "Address 3:")]
        public String MailingAddressLine3 { get; set; }

        [Display(Name = "City: ")]
        public string MailingCity { get; set; }

        [Display(Name = "State: ")]
        public string MailingState { get; set; }

        [Display(Name = "Zip Code: ")]
        public string MailingZipCode { get; set; }

        [Display(Name = "County: ")]
        public string MailingCounty { get; set; }

        [Display(Name = "Address 1: ")]
        public String PhysicalAddressLine1 { get; set; }

        [Display(Name = "Address 2:")]
        public String PhysicalAddressLine2 { get; set; }

        [Display(Name = "Address 3:")]
        public String PhysicalAddressLine3 { get; set; }

        [Display(Name = "City: ")]
        public string PhysicalCity { get; set; }

        [Display(Name = "State: ")]
        public string PhysicalState { get; set; }

        [Display(Name = "Code: ")]
        public string PhysicalZipCode { get; set; }

        [Display(Name = "County: ")]
        public string PhysicalCounty { get; set; }

        [Display(Name = "Home Phone: ")]
        public string HomePhone { get; set; }

        [Display(Name = "Mobile Phone: ")]
        public string MobilePhone { get; set; }

        [Display(Name = "Email Address: ")]
        public string EmailAddress { get; set; }

        [Display(Name = "Comm. Preference: ")]
        public string CommPreference { get; set; }

        [Display(Name = "Claim Status: ")]
        public string ClaimStatus { get; set; }

        [Display(Name = "Group Name: ")]
        public string GroupName { get; set; }

        [Display(Name = "Plan Name: ")]
        public string PlanName { get; set; }

        [Display(Name = "Coverage Type: ")]
        public string CoverageType { get; set; }

        [Display(Name="Market: ")]
        public string Market { get; set; }

        [Display(Name = "Coverage Start Date:")]
        public DateTime CoverageStartDate { get; set; }

        [Display(Name = "Monthly Cost: ")]
        public decimal MonthlyPremiumAmount { get; set; }


        
        public List<DependentInformation> Dependents { get; set; }
    }
}