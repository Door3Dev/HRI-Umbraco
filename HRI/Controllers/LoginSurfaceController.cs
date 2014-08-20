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
                    // Don't add a field level error, just model level
                    ModelState.AddModelError("loginModel", "Invalid username or password");
                    return CurrentUmbracoPage();
                }
                else // If the user doesn't exists, check the HRI API to see if this is a returning IWS user
                {
                    // Create a JSON object to receive the HRI API response
                    JObject json;
                    // Exectue a GET against the API
                    using(var client = new WebClient())
                    {
                        try
                        {
                            // Read the response into a string
                            string jsonString = client.DownloadString("http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/api/HriApi/GetRegisteredUserByUsername?userName=" + model.Username);
                            // If the user existed create a JSON object
                            if (jsonString != "null")
                                json = JObject.Parse(jsonString);
                            else // There is an API error
                            {
                                //don't add a field level error, just model level
                                ModelState.AddModelError("loginModel", "There was trouble accessing your account, please contact us by telephone.");
                                return CurrentUmbracoPage();
                            }
                        }
                        catch(Exception ex) // There was an error in connecting to or executing the function on the API
                        {
                            ModelState.AddModelError("loginModel", "Error in API call GetRegisteredUserByUsername");
                            return CurrentUmbracoPage();
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
                        if(json["Ssn"].HasValues)
                            registerModel.MemberProperties.Where(p => p.Alias == "ssn").FirstOrDefault().Value = json["Ssn"].ToString();
                        // SSN
                        if (json["EbixId"].HasValues)
                            registerModel.MemberProperties.Where(p => p.Alias == "ebixId").FirstOrDefault().Value = json["ebixID"].ToString();
                        // Email
                        if (json["EMail"].HasValues)
                            registerModel.Email = json["EMail"].ToString();
                        // Zip Code
                        if (json["ZipCode"].HasValues)
                        registerModel.MemberProperties.Where(p => p.Alias == "zipCode").FirstOrDefault().Value = json["ZipCode"].ToString();
                        // Phone Number
                        if (json["PhoneNumber"].HasValues)
                        registerModel.MemberProperties.Where(p => p.Alias == "phoneNumber").FirstOrDefault().Value = json["PhoneNumber"].ToString();
                        // Y Number
                        if (json["MemberId"].HasValues)
                        registerModel.MemberProperties.Where(p => p.Alias == "yNumber").FirstOrDefault().Value = json["MemberId"].ToString();

                        
                        registerModel.Password = Membership.GeneratePassword(12, 4);
                        registerModel.LoginOnSuccess = false;
                        registerModel.UsernameIsEmail = false;

                        // Register the user with Door3 automatically
                        MembershipCreateStatus status;
                        var newMember = Members.RegisterMember(registerModel, out status, registerModel.LoginOnSuccess);
                        // Force sign out (hack for Umbraco bug that automatically logs user in on registration
                        Session.Clear();
                        FormsAuthentication.SignOut();
                        
                        // Authenticate the user automatically as a registered user
                        newMember.IsApproved = true;
                        System.Web.Security.Roles.AddUserToRole(newMember.UserName, "Registered");
                        
                        // Reset the password and send an email to the user
                        bool resetSuccess;
                        string resetApiUrl = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/Surface/EmailSurface/ResetPassword?userName=" + model.Username;
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
            return Redirect("/member-center/index");
        }

    }
}