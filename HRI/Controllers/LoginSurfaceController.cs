using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Models;

namespace HRI.Controllers
{
    public class LoginSurfaceController : HriSufraceController
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
            var member = Services.MemberService.GetByUsername(model.Username); 

            // If the user is unable to login
            if (Members.Login(model.Username, model.Password) == false)
            {
                // Check to make sure that the user exists
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
                    if((string)json["RegId"] != null)
                    {
                        // Create the registration model
                        var registerModel = Members.CreateRegistrationModel();                        
                        // Member Name
                        registerModel.Name = json["FirstName"].ToString() + " " + json["LastName"].ToString();
                        // Member Id
                        registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "memberId").Value = json["RegId"].ToString();
                        // User Name
                        registerModel.Username = json["UserName"].ToString();
                        // First Name
                        registerModel.MemberProperties.FirstOrDefault(p => p.Alias=="firstName").Value = json["FirstName"].ToString();
                        // Last Name
                        registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "lastName").Value = json["LastName"].ToString();
                        // SSN
                        if((string)json["Ssn"] != null)
                            registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "ssn").Value = json["Ssn"].ToString();
                        // SSN
                        if ((string)json["EbixId"] != null)
                            registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "ebixId").Value = json["ebixID"].ToString();
                        // Email
                        if ((string)json["EMail"] != null)
                            registerModel.Email = json["EMail"].ToString();
                        // Zip Code
                        if ((string)json["ZipCode"] != null)
                            registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "zipCode").Value = json["ZipCode"].ToString();
                        // Phone Number
                        if ((string)json["PhoneNumber"] != null)
                            registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "phoneNumber").Value = json["PhoneNumber"].ToString();
                        // Y Number
                        if ((string)json["MemberId"] != null)
                            registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "yNumber").Value = json["MemberId"].ToString();

                        // Create a random Guid
                        Guid key = Guid.NewGuid();
                        // Update the user's Guid field
                        registerModel.MemberProperties.FirstOrDefault(p => p.Alias == "guid").Value = key.ToString();

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
                        Roles.AddUserToRole(newMember.UserName, "Registered");
                        
                        // Reset the password and send an email to the user
                        SendResetPasswordEmail(newMember.Email, newMember.UserName, key.ToString());
                        
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

            //
            if (member.GetValue<string>("enrollmentpageafterlogin") == "1")
            {
                member.SetValue("enrollmentpageafterlogin", String.Empty);
                Services.MemberService.Save(member);
                return Redirect("/your-account/enrollment/");
            }

            //if there is a specified path to redirect to then use it
            if (!string.IsNullOrEmpty(model.RedirectUrl))
            {
                return Redirect(model.RedirectUrl);
            }

            //redirect to current page by default
            TempData["LoginSuccess"] = true;
            return Redirect("/member-center/index");
        }

    }
}