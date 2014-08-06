using Newtonsoft.Json;
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
                var member = Services.MemberService.GetByUsername(model.Username);                
                if (member != null)
                {
                    // If the user does exist then it was a wrong password
                    //don't add a field level error, just model level
                    ModelState.AddModelError("loginModel", "Invalid username or password");
                    return CurrentUmbracoPage();
                }
                else // If the user doesnt exists, check the HRI API to see if this is a returning IWS user
                {

                    // TO-DO: use umbraco field to build address
                    string userNameCheckApiString = "http://23.253.132.105:64102/api/Registration?userName=" + model.Username;
                    string response;
                    JObject json;

                    Dictionary<string, string> jsonData = new Dictionary<string, string>();
                    jsonData.Add("userName", model.Username);
                    // Convert the dictionary to JSON
                    string myJsonString = (new JavaScriptSerializer()).Serialize(jsonData);
                    using(var client = new WebClient())
                    {
                        // Set the format to JSON
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        try
                        {
                            response = client.DownloadString(userNameCheckApiString);
                            json = JObject.Parse(response);
                        }
                        catch(WebException ex)
                        {                            
                            return Content("Error");
                        }
                    }                    
                    
                    // If the user exists in IWS database
                    if(json["RegId"] != null)
                    {

                        // Create the registration model
                        var registerModel = Members.CreateRegistrationModel();
                        
                        // Member Name
                        registerModel.Name = json["FirstName"].ToString() + " " + json["LastName"].ToString();
                        // Member Id
                        registerModel.MemberProperties.Where(p => p.Alias == "memberId").FirstOrDefault().Value = json["RegId"].ToString();
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
                        // Y Number
                        registerModel.MemberProperties.Where(p => p.Alias == "yNumber").FirstOrDefault().Value = json["MemberId"].ToString();

                        
                        registerModel.Password = "P@ssw0rd";
                        registerModel.LoginOnSuccess = false;
                        registerModel.UsernameIsEmail = false;

                        // Register the user with Door3 automatically
                        MembershipCreateStatus status;
                        var newMember = Members.RegisterMember(registerModel, out status, registerModel.LoginOnSuccess);
                        Session.Clear();
                        FormsAuthentication.SignOut();
                        
                        // Authenticate the user automatically as a registered user
                        newMember.IsApproved = true;
                        System.Web.Security.Roles.AddUserToRole(newMember.UserName, "Registered");
                        
                        // Reset the password and send an email to the user
                        bool resetSuccess;
                        string resetApiUrl = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/Surface/EmailSurface/ResetPassword?userName=" + model.Username + "&smtpServer=" + "smtp.live.com" + "&email=" + "mattwood2855@hotmail.com" + "&pass=" + "ChangePass1";
                        using(var client = new WebClient())
                        {
                            var result = client.DownloadString(resetApiUrl);
                            resetSuccess = Convert.ToBoolean(result);
                        }                            

                        if(resetSuccess)
                        {
                            return Redirect("/for-members/security-upgrade/");
                        }
                        else
                        {
                            return Redirect("/");
                        }
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