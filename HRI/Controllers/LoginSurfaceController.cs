using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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
                // If the user doesn't exists, check the HRI API to see if this is a returning IWS user
                JObject hriUser;
                try
                {
                    hriUser = MakeInternalApiCallJson("GetRegisteredUserByUsername",
                        new Dictionary<string, string> {{"userName", model.Username}});
                }
                catch // There was an error in connecting to or executing the function on the API
                {
                    ModelState.AddModelError("loginModel", "Error in API call GetRegisteredUserByUsername");
                    return CurrentUmbracoPage();
                }

                // There is an API error
                if (hriUser == null)
                {
                    ModelState.AddModelError("loginModel", "There was trouble accessing your account, please contact us by telephone.");
                    return CurrentUmbracoPage();
                }
                                                          
                // If the user exists in IWS database
                if((string)hriUser["RegId"] != null)
                {
                    // Before attempt to create a user need to check the email and login uniqueness
                    var existedUserWithUsername = Services.MemberService.GetByUsername(hriUser["UserName"].ToString());
                    var existedUserWithEmail = Services.MemberService.GetByEmail(hriUser["EMail"].ToString());
                    if (existedUserWithEmail != null || existedUserWithUsername != null)
                    {
                        ModelState.AddModelError("loginModel", "We cannot log you in with this user name. The email address of the user name you entered is associated with another user name. Please enter a valid user name and try again or contact Member Services for assistance.");
                        return CurrentUmbracoPage();
                    }

                    // Create the registration model
                    var registerModel = Members.CreateRegistrationModel();                        
                    // Member Name
                    registerModel.Name = hriUser["FirstName"].ToString() + " " + hriUser["LastName"].ToString();
                    // Member Id
                    registerModel.MemberProperties.First(p => p.Alias == "memberId").Value = hriUser["RegId"].ToString();
                    // User Name
                    registerModel.Username = hriUser["UserName"].ToString();
                    // First Name
                    registerModel.MemberProperties.First(p => p.Alias=="firstName").Value = hriUser["FirstName"].ToString();
                    // Last Name
                    registerModel.MemberProperties.First(p => p.Alias == "lastName").Value = hriUser["LastName"].ToString();
                    // SSN
                    if((string)hriUser["Ssn"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "ssn").Value = hriUser["Ssn"].ToString();
                    // SSN
                    if ((string)hriUser["EbixId"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "ebixId").Value = hriUser["ebixID"].ToString();
                    // Email
                    if ((string)hriUser["EMail"] != null)
                        registerModel.Email = hriUser["EMail"].ToString();
                    // Zip Code
                    if ((string)hriUser["ZipCode"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "zipCode").Value = hriUser["ZipCode"].ToString();
                    // Phone Number
                    if ((string)hriUser["PhoneNumber"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "phoneNumber").Value = hriUser["PhoneNumber"].ToString();
                    // Y Number
                    if ((string)hriUser["MemberId"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "yNumber").Value = hriUser["MemberId"].ToString();
                    // Group Id
                    if ((string)hriUser["RxGrpId"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "groupId").Value = hriUser["RxGrpId"].ToString();
                    // Birthday
                    if ((string) hriUser["DOB"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "birthday").Value = hriUser["DOB"].ToString();
                    // Plan Id
                    if ((string)hriUser["PlanId"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "healthplanid").Value = hriUser["PlanId"].ToString();
                    // Plan Name
                    if ((string)hriUser["PlanName"] != null)
                        registerModel.MemberProperties.First(p => p.Alias == "healthPlanName").Value = hriUser["PlanName"].ToString();

                    // Create a random Guid
                    Guid key = Guid.NewGuid();
                    // Update the user's Guid field
                    registerModel.MemberProperties.First(p => p.Alias == "guid").Value = key.ToString();

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
                    Roles.AddUserToRole(model.Username, "Enrolled");
                        
                    // Reset the password and send an email to the user
                    SendResetPasswordEmail(newMember.Email, newMember.UserName, key.ToString());
                        
                    return Redirect("/for-members/security-upgrade/");
                }
                // The user doesnt exist locally or in IWS db
                //don't add a field level error, just model level
                ModelState.AddModelError("loginModel", "Invalid username or password");
                return CurrentUmbracoPage();
            }

            // User should pass enrollment process
            if (member.GetValue<string>("enrollmentpageafterlogin") == "1")
            {
                // Each time when user trying to login and he is in the enrollment process
                // has no Group Id and Birthday
                // we should check the enrollment status through the API call
                var hriUser = MakeInternalApiCallJson("GetRegisteredUserByUsername",
                        new Dictionary<string, string> { { "userName", model.Username } });

                if (String.IsNullOrEmpty(hriUser.Value<string>("RxGrpId"))
                    || String.IsNullOrEmpty(hriUser.Value<string>("DOB")))
                {
                    return Redirect("/your-account/enrollment/");
                }
                // Save Group Id, Birthday, Plan Id, Plan Name, add user as enrolled
                member.Properties.First(p => p.Alias == "groupId").Value = hriUser["RxGrpId"].ToString();
                member.Properties.First(p => p.Alias == "birthday").Value = hriUser["DOB"].ToString();
                member.Properties.First(p => p.Alias == "healthplanid").Value = hriUser["PlanId"].ToString();
                member.Properties.First(p => p.Alias == "healthPlanName").Value = hriUser["PlanName"].ToString();
                Roles.AddUserToRole(model.Username, "Enrolled");

                member.SetValue("enrollmentpageafterlogin", String.Empty);
                Services.MemberService.Save(member);
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