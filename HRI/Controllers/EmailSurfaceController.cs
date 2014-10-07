using HRI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace HRI.Controllers
{
    public class EmailSurfaceController : HriSufraceController
    {
        /// <summary>
        /// Emails the Web Administrator with a message from a member
        /// </summary>
        /// <param name="model">A Contact Form view model</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ContactUs(ContactFormViewModel model)
        {
            try
            {
                var categoriesAndEmails =
                    ContactFormViewModel.GetCategoriesAndEmails(
                        CurrentPage.GetPropertyValue<string>("categoriesAndEmails"));

                var email =
                    categoriesAndEmails.First(
                        c => string.Compare(c.Item1, model.MessageType, StringComparison.Ordinal) == 0).Item2;

                const string na = "N/A";
                var message = string.Join(
                    Environment.NewLine,
                    string.Format("Member ID: {0}", model.MemberId ?? na),
                    string.Format("First Name: {0}", model.FirstName),
                    string.Format("Last Name: {0}", model.LastName),
                    string.Format("Y Number: {0}", model.YNumber ?? na),
                    string.Format("Username: {0}", model.Username ?? na),
                    string.Format("Phone Number: {0}", model.PhoneNumber),
                    "Message:",
                    Environment.NewLine,
                    model.Message);

                SendEmail(email, model.MessageType, message);

                // Set the sucess flag to true and post back to the same page
                TempData["IsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception) // If the message failed to send
            {
                // Set the success flag to false and post back to the same page
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ForgotUserName([Bind(Prefix = "forgotUserNameViewModel")]ForgotUserNameViewModel model)
        {
            try 
            {
                // Attempt to get the member based on the given email address
                var member = Services.MemberService.GetByEmail(model.Email);
                // If a member with that email exists
                if (member != null)
                {
                    // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
                    IPublishedContent root = Umbraco.TypedContentAtRoot().First();
                   
                    // Build a dictionary for all teh dynamic text in the email template
                    var dynamicText = new Dictionary<string,string>
                    {
                        {"<%FirstName%>", member.GetValue("firstName").ToString()},
                        {"<%UserName%>", member.Username},
                        {"<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString()}
                    };

                    //Get the Verification Email Template ID
                    var emailTemplateId = root.GetProperty("forgotUserNameEmailTemplate").Value;


                    SendEmail(member.Email, "Health Republic Insurance - Username Recovery",
                                             BuildEmail((int)emailTemplateId, dynamicText));

                    // Set the sucess flag to true and post back to the same page
                    TempData["ForgotUsernameIsSuccessful"] = true;
                    return RedirectToCurrentUmbracoPage();
                }
                // The email has no member associated with it
                // Set the success flag to false and post back to the same page
                TempData["ForgotUsernameIsSuccessful"] = false;
                TempData["EmailNotFound"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("forgotUserNameViewModel", ex.Message + "\n" + ex.InnerException.Message + "\n");
                // Set the success flag to false and post back to the same page
                TempData["ForgotUsernameIsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // If the username exists
                if (Services.MemberService.GetByUsername(model.UserName) != null)
                {
                    #region User Exists
                    // Get the member
                    var member = Services.MemberService.GetByUsername(model.UserName);
                    // Create a random Guid
                    Guid key = Guid.NewGuid();
                    // Update the user's Guid field
                    member.SetValue("guid", key.ToString());
                    // Save the updated information
                    Services.MemberService.Save(member);

                    SendResetPasswordEmail(member.Email, member.Username, key.ToString());

                    TempData["ForgotPasswordIsSuccessful"] = true;
                    return RedirectToCurrentUmbracoPage();
                    #endregion
                }

                #region User Not Found

                #region Checkolduser
                // If the user doesn't exists, check the HRI API to see if this is a returning IWS user
                JObject hriUser;
                try
                {
                    hriUser = MakeInternalApiCallJson("GetRegisteredUserByUsername",
                        new Dictionary<string, string> { { "userName", model.UserName } });
                }
                catch // There was an error in connecting to or executing the function on the API
                {
                    ModelState.AddModelError("model", "Error in API call GetRegisteredUserByUsername");
                    return CurrentUmbracoPage();
                }

                // There is an API error
                if (hriUser == null)
                {
                    ModelState.AddModelError("model", "Sorry, that user name does not exist in our system.");
                    return CurrentUmbracoPage();
                }

                // If the user exists in IWS database
                if ((string)hriUser["RegId"] != null)
                {
                    // Create the registration model
                    var registerModel = Members.CreateRegistrationModel();
                    // Member Name
                    registerModel.Name = hriUser["FirstName"].ToString() + " " + hriUser["LastName"].ToString();
                    // Member Id
                    registerModel.MemberProperties.First(p => p.Alias == "memberId").Value = hriUser["RegId"].ToString();
                    // User Name
                    registerModel.Username = hriUser["UserName"].ToString();
                    // First Name
                    registerModel.MemberProperties.First(p => p.Alias == "firstName").Value = hriUser["FirstName"].ToString();
                    // Last Name
                    registerModel.MemberProperties.First(p => p.Alias == "lastName").Value = hriUser["LastName"].ToString();
                    // SSN
                    if ((string)hriUser["Ssn"] != null)
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
                    {
                        registerModel.MemberProperties.First(p => p.Alias == "birthday").Value = hriUser["DOB"].ToString();
                        Roles.AddUserToRole(model.UserName, "Enrolled");
                    }
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
                    #endregion

                    // Reset the password and send an email to the user
                    SendResetPasswordEmail(newMember.Email, newMember.UserName, key.ToString());

                    TempData["ForgotPasswordIsSuccessful"] = true;
                    return RedirectToCurrentUmbracoPage();                      
                }
                return CurrentUmbracoPage();
                #endregion
            }
            // The model was invalid
            TempData["ForgotPasswordIsSuccessful"] = false;
            return RedirectToCurrentUmbracoPage();
        }


        /// <summary>
        /// This version is called from the resend email page. It sends them a new verification email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendVerificationLink([Bind(Prefix = "sendVerificationLinkModel")]SendVerificationLinkModel model)
        {
            // If the user model is valid and the user exists
            if (ModelState.IsValid && Services.MemberService.GetByUsername(model.UserName) != null)
            {  
                // If this user has already been verified
                if(Services.MemberService.GetByUsername(model.UserName).IsApproved)
                {
                    TempData["ResendEmailAlreadyVefiried"] = true;
                    return CurrentUmbracoPage();
                }
                // Get a handle on the member
                var member = Services.MemberService.GetByUsername(model.UserName);               
                // Create a random Guid
                Guid key = Guid.NewGuid();
                // Update the user's Guid field
                member.SetValue("guid", key.ToString());
                // Save the updated information
                Services.MemberService.Save(member);

                // Get ahold of the root/home node
                IPublishedContent root = Umbraco.ContentAtRoot().First();
                // Get the Verification Email Template ID
                var emailTemplateId = root.GetProperty("verificationEmailTemplate").Value;

                // Build a dictionary for all the dynamic text in the email template
                var dynamicText = new Dictionary<string, string>();
                dynamicText.Add("<%FirstName%>", member.GetValue("firstName").ToString());
                dynamicText.Add("<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString());
                dynamicText.Add("<%VerificationUrl%>", "http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/Surface/MembersSurface/ActivateUser?username=" + model.UserName + "&guid=" + key.ToString());

                // Try to send the message
                try
                {
                    SendEmail(member.Email, "Health Republic Insurance - Member Verification Link",
                                            BuildEmail((int)emailTemplateId, dynamicText));
                }
                catch(SmtpException ex)
                {
                    //don't add a field level error, just model level
                    ModelState.AddModelError("sendVerificationLinkModel", ex.Message + "\n" + ex.InnerException.Message + "\n");
                    return CurrentUmbracoPage();
                }

                // Mark this method as successful for the next page
                TempData["IsSuccessful"] = true;

                // If there is a redirect url
                if (!string.IsNullOrEmpty(model.RedirectUrl))
                    // Send the user to that page
                    return Redirect(model.RedirectUrl);
                return Redirect("/");
            }
            // Model was bad or user didnt exist
            // Mark the method as failed
            TempData["IsSuccessful"] = false;
            // Return the user to the home page
            return Redirect("/");
        }

        /// <summary>
        /// This is called from the registration page after a new user is registered
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SendVerificationLink_GET(SendVerificationLinkModel model)
        {
            if (ModelState.IsValid && Services.MemberService.GetByUsername(model.UserName) != null)
            {
                // Get a handle on the member
                var member = Services.MemberService.GetByUsername(model.UserName);
                // Create a random Guid
                Guid key = Guid.NewGuid();
                // Update the user's Guid field
                member.SetValue("guid", key.ToString());
                // Save the updated information
                Services.MemberService.Save(member);

                // Get ahold of the root/home node
                IPublishedContent root = Umbraco.ContentAtRoot().First();
                // Get the Verification Email Template ID
                var emailTemplateId = root.GetProperty("verificationEmailTemplate").Value;                

                // Build a dictionary for all the dynamic text in the email template
                var dynamicText = new Dictionary<string, string>();
                dynamicText.Add("<%FirstName%>", member.GetValue("firstName").ToString());
                dynamicText.Add("<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString());
                dynamicText.Add("<%VerificationUrl%>", root.GetProperty("HostUrl").Value.ToString() + "/umbraco/Surface/MembersSurface/ActivateUser?username=" + model.UserName + "&guid=" + key.ToString());

                // Try to send the message
                try
                {
                    SendEmail(member.Email, "Health Republic Insurance - Member Verification Link",
                                            BuildEmail((int)emailTemplateId, dynamicText));
                }
                catch (SmtpException ex)
                {
                    //don't add a field level error, just model level
                    ModelState.AddModelError("sendVerificationLinkModel", ex.Message + "\n" + ex.InnerException.Message + "\n");
                    return Redirect("/for-members/register");
                }

                // Mark this method as successful for the next page
                TempData["IsSuccessful"] = true;

                // If there is a redirect url
                if (!string.IsNullOrEmpty(model.RedirectUrl))
                    // Send the user to that page
                    return Redirect(model.RedirectUrl);
                // Otherwise send the user to the home page
                return Redirect("/");
            }
            // Model was bad or user didnt exist
            // Mark the method as failed
            TempData["IsSuccessful"] = false;
            // Return the user to the home page
            return Redirect("/");
        }
    }
}