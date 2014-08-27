using System.Linq;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using Umbraco.Web.WebApi;
using Umbraco.Core.Models;
using Newtonsoft.Json;

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
            var valuesList  = values.Select(_ => String.Format("{0}={1}", HttpUtility.UrlEncode(_.Key), HttpUtility.UrlEncode(_.Value)));
            string userNameCheckApiString = apiUri + "/Registration?" + String.Join("&", valuesList);
            // Create a web client to access the API
            try
            {
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks to see if a username is available using the HRI API
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet]
        public bool IsUserNameAvailable(string username)
        {
            var result = MakeApiCall(new Dictionary<string, string> {{"isUserNameAvailable", username}});

            return result != null && Convert.ToBoolean(result["isAvailable"]);
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
        /// Registers a user with the HRI web API
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public string RegisterUser(string userName)
        {
            // Get an instance of the member
            var member = Services.MemberService.GetByUsername(userName);
            // Create a dictionary of the members info that we will convert to JSON and send to HRI API
            Dictionary<string, string> jsonData = new Dictionary<string, string>();
            jsonData.Add("RegId", null);
            jsonData.Add("RegDate", DateTime.Now.ToString());
            // The MemberId is null for the new user
            jsonData.Add("MemberId", member.GetValue("yNumber").ToString());
            jsonData.Add("UserName", member.Username);
            jsonData.Add("FirstName", member.GetValue("firstName").ToString());
            jsonData.Add("LastName", member.GetValue("lastName").ToString());
            jsonData.Add("Ssn", member.GetValue("ssn").ToString());
            jsonData.Add("EMail", member.Email);
            jsonData.Add("ZipCode", member.GetValue("zipCode").ToString());
            jsonData.Add("PhoneNumber", member.GetValue("phoneNumber").ToString());
            jsonData.Add("RegVerified", "true");
            // Convert the dictionary to JSON
            string myJsonString = (new JavaScriptSerializer()).Serialize(jsonData);

            // Get ahold of the root/home node
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            // Apend the command to invoke the register function
            string registerUserApi = apiUri + "/Registration";                                   
            
            // Create a JSON object to hold the response
            JObject json = new JObject();

            // Create a webclient object to post the user data
            using(var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                
                // Get the response when posting the member
                try
                {
                    string response = client.UploadString(registerUserApi, myJsonString);
                    json = JObject.Parse(response);
                }
                catch(WebException ex)
                {

                    // If there was an error, return an error and message in the JSON string
                    json.Add("error", "true");
                    json.Add("message", ex.Message + ". " + ex.InnerException);                    
                }
            }

            // If the user was created
            if (json["MemberId"] != null)
            {
                // Assign this user their member id                
                member.SetValue("memberId", json["RegId"].ToString());                
                // Assign their Morneau Shapell Y Number
                member.SetValue("yNumber", json["MemberId"].ToString());
                Services.MemberService.Save(member);
                // Return successful registration
                json.Add("error", "false");
                return json.ToString(Formatting.None);
            }

            // Member was not registered with HRI           
            return json.ToString(Formatting.None);
        }

        /// <summary>
        /// Get the given users ebix id.
        /// </summary>
        /// <param name="username">Name of the user to retrieve Ebix Id for</param>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public string GetEbixIdByYNumber(string username)
        {
            string ynumber = Services.MemberService.GetByUsername(username).GetValue("yNumber").ToString();
            var result = MakeApiCall(new Dictionary<string, string> { { "EbixMemberId", ynumber } });

            return result["EBIXMemberId"].Value<string>();
        }
    }
}