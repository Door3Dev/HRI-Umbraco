using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class HriLoginSurfaceController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")]LoginModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            // If the user is unable to login
            if (Members.Login(model.Username, model.Password) == false)
            {
                // Check to make sure that the user exists
                var member = Members.GetByUsername(model.Username);                
                if (member != null)
                {
                    // If the user does exist then it was a wrong password
                    //don't add a field level error, just model level
                    ModelState.AddModelError("loginModel", "Invalid username or password");
                    return CurrentUmbracoPage();
                }
                else // If the user doesnt exists, check the HRI API to see if this is a returning IWS user
                {
                    // Create a API url
                    // TO-DO: use umbraco field to build address
                    string userNameCheckApiString = "http://23.253.132.105:64102/api/Registration?isMemberIdRegistered=" + model.Username;
                    
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
                    
                    // If the user exists in IWS database
                    if(Convert.ToBoolean(json["Registered"]))
                    {
                        // Create the user locally
                        // Force them to change their password
                    }
                    else // The user doesnt exist locally or in IWS db
                    {
                        //don't add a field level error, just model level
                        ModelState.AddModelError("loginModel", "Invalid username or password");
                        return CurrentUmbracoPage();
                    }
                }                
            }

            //if there is a specified path to redirect to then use it
            if (model.RedirectUrl != null && model.RedirectUrl != "")
            {
                return Redirect(model.RedirectUrl);
            }

            //redirect to current page by default
            TempData["LoginSuccess"] = true;
            return RedirectToCurrentUmbracoPage();
            //return RedirectToCurrentUmbracoUrl();
        }

    }
}