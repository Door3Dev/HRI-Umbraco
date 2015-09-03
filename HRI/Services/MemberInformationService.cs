using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

using HRI.Models;
using HRI.ViewModels;

namespace HRI.Services
{
    public class MemberInformationService
    {
        private string apiUri = string.Empty;
        private IContentService _contentService;

        public MemberInformationService()
        {
            _contentService = ApplicationContext.Current.Services.ContentService;

            // Get ahold of the root/home node
            var root = _contentService.GetRootContent().First();
            // Get the API uri
            apiUri = root.GetValue<string>("apiUri");
        }


        internal SubscriberInformation GetMemberInformation(string memberId)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(apiUri + "/MemberInformation/GetMemberInformation?memberId=" + memberId);   
            req.ContentType = "application/json";
            req.Method = "Get";
            req.AllowAutoRedirect = false;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            String output = RespsoneToString(resp);
            try
            {
                if (output == "null" || string.IsNullOrEmpty(output))
                {
                    return null;
                }
                else
                {
                    System.Web.Script.Serialization.JavaScriptSerializer jser = new System.Web.Script.Serialization.JavaScriptSerializer();
                    object obj = jser.Deserialize(output, typeof(SubscriberInformation));
                    SubscriberInformation result = (SubscriberInformation)obj;
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string RespsoneToString(HttpWebResponse response)
        {
            Stream receiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(receiveStream, encode);
            string output = readStream.ReadToEnd();
            return output;

        }

        internal MembershipInformationViewModel GetMemberInformationViewModel(SubscriberInformation subInfo)
        {
            MembershipInformationViewModel model = new MembershipInformationViewModel();
            model.MemberId = subInfo.MemberId;
            model.MemberName = subInfo.FirstName + " " + subInfo.LastName;

            if (!string.IsNullOrEmpty(subInfo.SSN))
            {
                model.SSN = "XXX-XX-" + subInfo.SSN.Substring(5, 4);
            }

            model.SubscriberFlag = subInfo.SubscriberFlag;
            model.EnrolledAs = subInfo.SubscriberFlag == "Y" ? "Subscriber" : "Dependent";

            if (!string.IsNullOrEmpty(subInfo.Gender))
            {
                model.Gender = subInfo.Gender == "M" ? "Male" : "Female";
            }

            if (subInfo.DOB < DateTime.Today)
            {
                model.DOB = subInfo.DOB;
            }

            if (!string.IsNullOrEmpty(subInfo.MaritalStatus))
            {
                model.MaritalStatus = subInfo.MaritalStatus;
            }

            model.Relationship = subInfo.Relationship;
            model.MailingAddressLine1 = subInfo.MailingAddressLine1;
            model.MailingAddressLine2 = subInfo.MailingAddressLine2;
            model.MailingAddressLine3 = subInfo.MailingAddressLine3;
            model.MailingCity = subInfo.MailingCity;
            model.MailingState = subInfo.MailingState;

            if (!string.IsNullOrEmpty(subInfo.MailingZipCode))
            {
                if (subInfo.MailingZipCode.Length > 5)
                {
                    model.MailingZipCode = string.Format("{0}-{1}", subInfo.MailingZipCode.Substring(0, 5), subInfo.MailingZipCode.Substring(5));
                }
                else
                {
                    model.MailingZipCode = subInfo.MailingZipCode;
                }
            }

            model.PhysicalAddressLine1 = subInfo.PhysicalAddressLine1;
            model.PhysicalAddressLine2 = subInfo.PhysicalAddressLine2;
            model.PhysicalAddressLine3 = subInfo.PhysicalAddressLine3;
            model.PhysicalCity = subInfo.PhysicalCity;
            model.PhysicalState = subInfo.PhysicalState;

            if (!string.IsNullOrEmpty(subInfo.PhysicalZipCode))
            {
                if (subInfo.PhysicalZipCode.Length > 5)
                {
                    model.PhysicalZipCode = string.Format("{0}-{1}", subInfo.PhysicalZipCode.Substring(0, 5), subInfo.PhysicalZipCode.Substring(5));
                }
                else
                {
                    model.PhysicalZipCode = subInfo.PhysicalZipCode;
                }
            }

            if (!string.IsNullOrEmpty(subInfo.HomePhone))
            {
                if (subInfo.HomePhone.Length == 10)
                {
                    model.HomePhone = string.Format("{0}-{1}-{2}", subInfo.HomePhone.Substring(0, 3), subInfo.HomePhone.Substring(3, 3), subInfo.HomePhone.Substring(6, 4));
                }
            }

            if (!string.IsNullOrEmpty(subInfo.MobilePhone))
            {
                if (subInfo.MobilePhone.Length == 10)
                {
                    model.MobilePhone = string.Format("{0}-{1}-{2}", subInfo.MobilePhone.Substring(0, 3), subInfo.MobilePhone.Substring(3, 3), subInfo.MobilePhone.Substring(6, 4));
                }
            }

            model.EmailAddress = subInfo.EmailAddress;
            model.CommPreference = subInfo.CommunicationPreference;

            model.ClaimStatus = subInfo.Status;
            model.GroupName = subInfo.GroupName;
            model.PlanName = subInfo.PlanName;
            model.Market = subInfo.Market;
            model.CoverageStartDate = subInfo.EnrolledOn;
            model.MonthlyPremiumAmount = (decimal)subInfo.PremiumAmount;
            model.CoverageType = subInfo.CoverageType;

            if (subInfo.Dependents.Count > 0)
            {
                foreach (DependentInformation dep in subInfo.Dependents)
                {
                    dep.Gender = dep.Gender == "M" ? "Male" : "Female";
                }
            }
            model.Dependents = subInfo.Dependents;
            return model;
        }
    }
}