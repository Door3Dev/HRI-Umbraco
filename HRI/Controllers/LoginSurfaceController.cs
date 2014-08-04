using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class LoginSurfaceController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")]LoginModel model)
        {
            // If the model is NOT valid
            if (ModelState.IsValid == false)
            {
                // Return the user to the login page
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
                    string userNameCheckApiString = "http://23.253.132.105:64102/api/Registration?userName=" + model.Username;
                    
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
                    if(json["RegId"] != null)
                    {

                        // Create the registration model
                        var registerModel = Members.CreateRegistrationModel();
                        
                        // Member Name
                        registerModel.Name = json["FirstName"].ToString() + " " + json["LastName"].ToString();
                        // Member Id
                        registerModel.MemberProperties.Where(p => p.Alias == "memberId").FirstOrDefault().Value = json["MemberId"].ToString();
                        // User Name
                        registerModel.Username = json["UserName"].ToString();
                        // First Name
                        registerModel.MemberProperties.Where(p=>p.Alias=="firstName").FirstOrDefault().Value = json["FirstName"].ToString();
                        // Last Name
                        registerModel.MemberProperties.Where(p => p.Alias == "lastName").FirstOrDefault().Value = json["LastName"].ToString();
                        // SSN
                        registerModel.MemberProperties.Where(p => p.Alias == "ssn").FirstOrDefault().Value = json["Ssn"].ToString();
                        // Email
                        registerModel.Email = json["EMail"].ToString();
                        // Zip Code
                        registerModel.MemberProperties.Where(p => p.Alias == "zipCode").FirstOrDefault().Value = json["ZipCode"].ToString();
                        // Phone Number
                        registerModel.MemberProperties.Where(p => p.Alias == "phoneNumber").FirstOrDefault().Value = json["PhoneNumber"].ToString();

                        
                        registerModel.Password = "P@ssw0rd";
                        registerModel.LoginOnSuccess = false;
                        registerModel.UsernameIsEmail = false;

                        // Register the user with Door3 automatically
                        MembershipCreateStatus status;
                        var newMember = Members.RegisterMember(registerModel, out status, registerModel.LoginOnSuccess);
                        // Set the member to not approved (they must click on the verification link
                        newMember.IsApproved = false;

                        // Redirect the user to the 'security upgrade' page that lets them know to change their password
                        TempData["UserName"] = registerModel.Username;
                        TempData["RedirectUrl"] = "/";

                        return Redirect("/for-members/security-upgrade/");
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