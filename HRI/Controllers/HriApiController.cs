using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace HRI.Controllers
{
    public class HriApiController : UmbracoApiController
    {
        /// <summary>
        /// General method to make an HRI API call
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private JObject MakeApiCall(Dictionary<string, string> values)
        {
            // Get ahold of the root/home node
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            
            // Apend the command to determine user availability
            var valuesList = values.Select(_ => String.Format("{0}={1}", HttpUtility.UrlEncode(_.Key), HttpUtility.UrlEncode(_.Value)));

            string userNameCheckApiString = apiUri + "/Registration?" + String.Join("&", valuesList);
            // Create a web client to access the API
            // Create a JSON object to hold the response 
            JObject json;
            using (var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Execute a GET and convert the response to a JSON object
                json = JObject.Parse(client.DownloadString(userNameCheckApiString));
            }
            // Return whether or not it is available
            return json;
        }

        /// <summary>
        /// Checks to see if a username is available using the HRI API
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet]
        public bool IsUserNameAvailable(string username)
        {
            if (Regex.IsMatch(username, "['\";!@#$%^&*]"))
            {
                return false;
            }
            var resultHri = MakeApiCall(new Dictionary<string, string> {{"isUserNameAvailable", username}});
            var hriAvailability = resultHri != null && Convert.ToBoolean(resultHri["isAvailable"]);
            var localUser = Services.MemberService.GetByUsername(username);

            return hriAvailability && localUser == null;
        }

        /// <summary>
        /// Checks to see if a memberId is available using the HRI API
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpGet]
        public bool IsMemberIdRegistered(string memberId)
        {
            var result = MakeApiCall(new Dictionary<string, string> { { "isMemberIdRegistered", memberId } });

            return result != null && Convert.ToBoolean(result["Registered"]);
        }
        
        /// <summary>
        /// Checks to see if a email is available using the HRI API
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        public bool IsEmailAddressInUse(string email)
        {
            var result = MakeApiCall(new Dictionary<string, string> { { "isEMailAddressInUse", email } });

            return result != null && Convert.ToBoolean(result["EmaiInUse"]); // Yes, "Emai"
        }

        [HttpGet]
        public JObject GetRegisteredUserByEmail(string email)
        {
            var result = MakeApiCall(new Dictionary<string, string> { { "eMail", email } });
            var hasRegId = result.Value<int?>("RegId").HasValue;

            return hasRegId ? result : null;
        }

        [HttpGet]
        public JObject GetRegisteredUserByUsername(string username)
        {
            var result = MakeApiCall(new Dictionary<string, string> { { "userName", username } });
            var hasRegId = result.Value<int?>("RegId").HasValue;

            return hasRegId ? result : null;
        }

        /// <summary>
        /// Get user by member id (Y number)
        /// </summary>
        /// <param name="memberId">Member Id (Y numbaer)</param>
        /// <returns>User data</returns>
        [HttpGet]
        public JObject GetRegisteredUserByMemberId(string memberId)
        {
            var result = MakeApiCall(new Dictionary<string, string> {{"memberId", memberId}});
            var hasRegId = result.Value<int?>("RegId").HasValue;

            return hasRegId ? result : null;
        }

        [HttpGet]
        public JObject IsUserWithMemberIdRegistered(string memberId)
        {
            var user = GetRegisteredUserByMemberId(memberId);
            if (user == null)
                return null;

            return JObject.FromObject(new
            {
                username = user.Value<string>("UserName"),
            });
        }

        /// <summary>
        /// Get the given users ebix id.
        /// </summary>
        /// <param name="username">Name of the user to retrieve Ebix Id for</param>
        /// <returns></returns>
        [AcceptVerbs("GET", "POST")]
        public string GetEbixIdByUserName(string username)
        {
            string ynumber = Services.MemberService.GetByUsername(username).GetValue("yNumber").ToString();
            JObject result = MakeApiCall(new Dictionary<string, string> {{"EbixMemberId", ynumber}});
            JToken id = result["EBIXMemberId"];
            if (id == null)
                throw new InvalidOperationException(string.Format("There is no EBIXMemberId for YNumber '{0}' (user '{1}').", ynumber, username));

            return id.Value<string>();
        }

        [HttpGet]
        public bool IsEnrolledByMemberId(string memberId, string DOB)
        {
            var keyAction = String.IsNullOrWhiteSpace(DOB) ? "isEnrollByMemberID" : "isEnrollMemberID";
            var result = MakeApiCall(new Dictionary<string, string> { { keyAction, memberId }, { "DOB", DOB }});
            return result.Value<bool>("Enrolled");
        }

        [HttpGet]
        public string GetHealthPlanIdByMemberId(string memberId)
        {
            var result = MakeApiCall(new Dictionary<string, string> { { "HealthPlan", memberId } });
            return result.Value<string>("planId");
        }
    }
}