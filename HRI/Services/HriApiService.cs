using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace HRI.Services
{
    public class HriApiService
    {
        private readonly IContentService _contentService = ApplicationContext.Current.Services.ContentService;
        private readonly IMemberService _memberService = ApplicationContext.Current.Services.MemberService;
        private readonly UmbracoHelper _umbracoHelper = new UmbracoHelper(UmbracoContext.Current);


        /// <summary>
        /// Check if email is in use
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool EmailIsInUse(string email)
        {
            var checkResult = Get("Registration", new Dictionary<string, string> {{"isEMailAddressInUse", email}});
            return (bool) checkResult.EmaiInUse;
        }

        /// <summary>
        /// Update user email
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="newEmail">New email</param>
        /// <returns></returns>
        public bool UpdateUserEmail(IMember member, string newEmail)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                var values = new Dictionary<string, string>
                {
                    { "MemberId", member.GetValue<string>("yNumber") }, 
                    { "EmailAddress", newEmail }
                };

                var jsonContent = new JavaScriptSerializer().Serialize(values);
              
                // Get ahold of the root/home node
                var root = _contentService.GetRootContent().First();
                // Get the API uri
                var apiUri = root.GetValue<string>("apiUri");
                // Apend the command to invoke the register function
                var url = apiUri + "/Registration?p1=" + member.Email;

                var response = client.UploadString(url, jsonContent);
            }
            return true;
        }

        /// <summary>
        /// Register user on the HRI side
        /// </summary>
        /// <param name="id">Umbraco user Id</param>
        /// <returns></returns>
        public IMember RegisterUser(int id)
        {
            // Get an instance of the member
            var member = _memberService.GetById(id);
            // Create a dictionary of the member's info that we will convert to JSON and send to HRI API
            var values = new Dictionary<string, string>
            {
                {"RegId", null},
                {"RegDate", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
                {"MemberId", member.GetValue<string>("yNumber")},
                {"UserName", member.Username},
                {"FirstName", member.GetValue<string>("firstName")},
                {"LastName", member.GetValue<string>("lastName")},
                {"Ssn", member.GetValue<string>("ssn")},
                {"EMail", member.Email},
                {"ZipCode", member.GetValue<string>("zipCode")},
                {"PhoneNumber", member.GetValue<string>("phoneNumber")},
                {"RegVerified", "true"}
            };

            var response = Post("Registration", values);

            // If the user was created
            if (response["MemberId"] != null)
            {
                UpdateMemberProperties(member);
                _memberService.Save(member);
            }

            return member;
        }

        /// <summary>
        /// Update additional member data
        /// </summary>
        /// <param name="member"></param>
        private void UpdateMemberProperties(IMember member)
        {
            var hriUser = GetRegisteredUserByUsername(member.Username);

            // Assign this user their member id                
            member.SetValue("memberId", hriUser["RegId"].ToString());
            // Assign their Morneau Shapell Y Number
            member.SetValue("yNumber", hriUser["MemberId"].ToString());
            member.SetValue("market", hriUser["Market"].ToString());
            if ((string)hriUser["PlanEffectiveDate"] != null)
            {
                member.SetValue("effectiveYear", hriUser["PlanEffectiveDate"].ToString());
            }
            if ((string)hriUser["EbixId"] != null)
            {
                member.SetValue("ebixId", hriUser["EbixId"].ToString());
            }
            if ((string)hriUser["RxGrpId"] != null)
            {
                member.SetValue("groupId", hriUser["RxGrpId"].ToString());
            }
            // Birthday
            if ((string)hriUser["DOB"] != null)
            {
                member.SetValue("birthday", hriUser["DOB"].ToString());
            }
            // Plan Id
            if ((string)hriUser["PlanId"] != null)
            {
                member.SetValue("healthplanid", hriUser["PlanId"].ToString());
            }
            // Plan Name
            if ((string)hriUser["PlanName"] != null)
            {
                member.SetValue("healthPlanName", hriUser["PlanName"].ToString());
            }
            // Morneau Shepell First Name
            if ((string)hriUser["MSFirstName"] != null)
            {
                member.SetValue("msFirstName", hriUser["MSFirstName"].ToString());
            }
            // Morneau Shepell Last Name
            if ((string)hriUser["MSLastName"] != null)
            {
                member.SetValue("msLastName", hriUser["MSLastName"].ToString());
            }
        }

        /// <summary>
        /// Get HRI registered user by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private JObject GetRegisteredUserByUsername(string username)
        {
            var result = Get("Registration", new Dictionary<string, string> { { "userName", username } });
            var hasRegId = result.Value<int?>("RegId").HasValue;

            return hasRegId ? result : null;
        }

        /// <summary>
        /// GET HRI API call
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="values">Query parameters</param>
        /// <returns></returns>
        private dynamic Get(string action, Dictionary<string, string> values)
        {
            // Get ahold of the root/home node
            IPublishedContent root = _umbracoHelper.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();

            // Apend the command to determine user availability
            var valuesList = values.Select(_ => string.Format("{0}={1}", HttpUtility.UrlEncode(_.Key), HttpUtility.UrlEncode(_.Value)));

            string apiFullUrl = string.Format("{0}/{1}?{2}", apiUri, action, String.Join("&", valuesList));
            // Create a web client to access the API
            // Create a JSON object to hold the response 
            dynamic json;
            using (var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Execute a GET and convert the response to a JSON object
                json = JObject.Parse(client.DownloadString(apiFullUrl));
            }
            // Return whether or not it is available
            return json;
        }

        /// <summary>
        /// POST HRI API call
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="values">Query parameters</param>
        /// <returns></returns>
        private JObject Post(string action, Dictionary<string, string> values)
        {
            // Get ahold of the root/home node
            IPublishedContent root = _umbracoHelper.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            // Create a dictionary of the members info that we will convert to JSON and send to HRI API
            var myJsonString = (new JavaScriptSerializer()).Serialize(values);

            // Apend the command to determine user availability
            var valuesList = values.Select(_ => string.Format("{0}={1}", HttpUtility.UrlEncode(_.Key), HttpUtility.UrlEncode(_.Value)));

            string apiFullUrl = string.Format("{0}/{1}?{2}", apiUri, action, String.Join("&", valuesList));
            // Create a web client to access the API
            // Create a JSON object to hold the response 
            JObject json;
            using (var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Execute a GET and convert the response to a JSON object
                string response = client.UploadString(apiFullUrl, myJsonString);
                json = JObject.Parse(response);
            }
            // Return whether or not it is available
            return json;
        }

    }
}