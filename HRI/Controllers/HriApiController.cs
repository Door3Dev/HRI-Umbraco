using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Umbraco.Web.Models;
using Umbraco.Web.WebApi;
using System.Net.Http;
using Umbraco.Core.Models;
using Umbraco.Web.Security;
using Newtonsoft.Json;
using System.Diagnostics;

namespace HRI.Controllers
{
    public class HriApiController : UmbracoApiController
    {

        /// <summary>
        /// Checks to see if a username is available using the HRI API
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public bool IsUserNameAvailable(string username)
        {
            // Get ahold of the root/home node
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            // Apend the command to determine user availability
            string userNameCheckApiString = apiUri + "/Registration?isUserNameAvailable=" + username;
            // Create a JSON object to hold the response 
            JObject json;
            // Create a web client to access the API
            try
            {
                using (var client = new WebClient())
                {
                    // Set the format to JSON
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    // Execute a GET and convert the response to a JSON object
                    json = JObject.Parse(client.DownloadString(userNameCheckApiString));
                }
                // Return whether or not it is available
                return Convert.ToBoolean(json["isAvailable"]);
            }
            catch(WebException ex)
            {
                return false;
            }
        }

        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public string GetRegisteredUserByUsername(string username)
        {
            // Get ahold of the root/home node
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            // Apend the command to determine user exists
            string userNameCheckApiString = apiUri + "/Registration?userName=" + username;
            string result;
            JObject json;

            using (var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Execute a GET and convert the response to a JSON object
                result = client.DownloadString(userNameCheckApiString);                
            }
            json = JObject.Parse(result);
            // If the user didn't exist
            var temp = json["RegId"];
            if(!json["RegId"].HasValues)
            {
                return null;
            }
            return result;
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

                    Trace.WriteLine("HRI-API-Register");
                    Trace.TraceError(ex.Message);
                    Trace.TraceError(ex.InnerException.Message);

                    json.Add("error", "true");
                    json.Add("message", ex.Message + ". " + ex.InnerException);                    
                }
            }

            // If the user was created
            if (json["MemberId"] != null)
            {
                // Assign this user their member id                
                member.SetValue("memberId", json["RegId"]);                
                // Assign their Morneau Shapell Y Number
                member.SetValue("yNumber", json["MemberId"]);
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
            // Get ahold of the root/home node
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            string ynumber = Services.MemberService.GetByUsername(username).GetValue("yNumber").ToString();

            // Apend the command to determine user exists
            string userNameCheckApiString = apiUri + "/Registration?EbixMemberId=" + ynumber;
            string result;
            JObject json;

            using (var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Execute a GET and convert the response to a JSON object
                result = client.DownloadString(userNameCheckApiString);
            }
            json = JObject.Parse(result);
            // If the user didn't exist
            string temp = json["EBIXMemberId"].Value<string>();
            return temp;
        }

        /// <summary>
        /// Get the given users plan id.
        /// </summary>
        /// <param name="username">Name of the user to retrieve Plan Id for</param>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public string GetPlanIdByUsername(string username)
        {
            // Get ahold of the root/home node
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the API uri
            string apiUri = root.GetProperty("apiUri").Value.ToString();
            // Apend the command to determine user exists
            string userNameCheckApiString = apiUri + "/Registration?PlanId=" + username;
            string result;
            JObject json;

            using (var client = new WebClient())
            {
                // Set the format to JSON
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // Execute a GET and convert the response to a JSON object
                result = client.DownloadString(userNameCheckApiString);
            }
            json = JObject.Parse(result);
            // If the user didn't exist
            string temp = json["PlanId"].ToString();
            if (!json["PlanId"].HasValues)
            {
                return null;
            }
            return temp;
        }
    }
}