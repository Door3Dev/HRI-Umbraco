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
            string userNameCheckApiString = "http://23.253.132.105:64102/api/Registration?isUserNameAvailable=" + username;
                    
            // Create a web request
            WebRequest request = WebRequest.Create(userNameCheckApiString);
            request.Method = "GET";
            request.Credentials = CredentialCache.DefaultCredentials;  
                    
            // Get the web response as a JSON object
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(receiveStream, encode);
            JObject json = JObject.Parse(readStream.ReadToEnd());
            response.Close();              
            readStream.Close();
            return Convert.ToBoolean(json["isAvailable"]);
        }

        /// <summary>
        /// Registers a user with the HRI web API
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public bool RegisterUser(string userName)
        {
            var model = Services.MemberService.GetByUsername(userName);

            // Create a dictionary for easy visualization of API object
            Dictionary<string, string> jsonData = new Dictionary<string, string>();
            jsonData.Add("RegId", null);
            jsonData.Add("RegDate", DateTime.Now.ToString());
            jsonData.Add("MemberId", null);
            jsonData.Add("UserName", model.Username);
            jsonData.Add("FirstName",  model.GetValue("firstName").ToString()); 
            jsonData.Add("LastName", model.GetValue("lastName").ToString());
            jsonData.Add("Ssn", model.GetValue("ssn").ToString());
            jsonData.Add("EMail", model.Email);
            jsonData.Add("ZipCode",  model.GetValue("zipCode").ToString());
            jsonData.Add("PhoneNumber", model.GetValue("phoneNumber").ToString());
            jsonData.Add("RegVerified", "true");

            // Convert the dictionary to JSON
            string myJsonString = (new JavaScriptSerializer()).Serialize(jsonData);

            // TO-DO bring this string to umbraco back end instead of hard coded
            string registerUserApi = "http://23.253.132.105:64102/api/Registration";

            JObject json;
            using(var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var response = client.UploadString(registerUserApi, myJsonString);
                json = JObject.Parse(response);
            }


            if (json["RegId"] != null)
            {
                Services.MemberService.GetByUsername(model.Username).SetValue("yNumber", json["yNumber"]);
            }

            return Convert.ToBoolean(json["isAvailable"]);
        }
    }
}