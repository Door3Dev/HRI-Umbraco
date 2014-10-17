using System.Web.Security;
using HRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web;
using log4net;

namespace HRI.Controllers
{
    public class EmailSurfaceController : HriSufraceController
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(EmailSurfaceController));

        private const string UsernameDoesNotExist = "Sorry, that user name does not exist in our system.";

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
                string mailData = CurrentPage.GetPropertyValue<string>("categoriesAndEmails");
                IDictionary<string, IEnumerable<string>> categoriesAndEmails = ContactFormViewModel.GetCategoriesAndEmails(mailData);
                IEnumerable<string> emails = categoriesAndEmails[model.MessageType];

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

                foreach (string email in emails)
                    try
                    {
                        SendEmail(email, model.MessageType, model.Message);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Send failed to " + email, ex);
                    }

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
                Dictionary<string,string> dynamicText = new Dictionary<string, string>();
                // Attempt to get the member based on the given email address
                var member = Services.MemberService.GetByEmail(model.Email);
                
                // If a member with that email exists
                if (member != null)
                {
                    // Build a dictionary for all teh dynamic text in the email template
                    dynamicText = new Dictionary<string,string>
                    {
                        {"<%FirstName%>", member.GetValue("firstName").ToString()},
                        {"<%UserName%>", member.Username}
                    };
                }
                else
                {
                    // try get user from API
                    var hriUser = MakeInternalApiCallJson("GetRegisteredUserByEmail", new Dictionary<string, string> { { "email", model.Email } });
                    if (hriUser != null)
                    {
                        dynamicText = new Dictionary<string, string>
                        {
                            {"<%FirstName%>", hriUser["FirstName"].ToString()},
                            {"<%UserName%>", hriUser["UserName"].ToString()}
                        };
                    }
                }

                if (dynamicText.Any())
                {
                    // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
                    IPublishedContent root = Umbraco.TypedContentAtRoot().First();

                    // add phone number to dynamic text
                    dynamicText.Add("<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString());

                    //Get the Verification Email Template ID
                    var emailTemplateId = root.GetProperty("forgotUserNameEmailTemplate").Value;
                    
                    SendEmail(model.Email, "Health Republic Insurance - Username Recovery",
                        BuildEmail((int) emailTemplateId, dynamicText));

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
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            // Get the member
            var member = Services.MemberService.GetByUsername(model.UserName);

            if (member == null)
            {
                // If the user doesn't exists, check the HRI API to see if this is a returning IWS user
                var currentUmbracoPage = InitiateSecurityUpgradeForIwsUser("model", model.UserName);
                if (currentUmbracoPage != null)
                {
                    return currentUmbracoPage;
                }

                // There is an API error
                ModelState.AddModelError("model", UsernameDoesNotExist);
                return CurrentUmbracoPage();
            }

            if (member.IsApproved && !Roles.IsUserInRole(model.UserName, "Registered"))
            { // User is in process of security upgrade
                return SendResetPasswordEmailAndRedirectToSecurityUpgradePage(model.UserName);
            }

            // Create a random Guid
            Guid key = Guid.NewGuid();
            // Update the user's Guid field
            member.SetValue("guid", key.ToString());
            // Save the updated information
            Services.MemberService.Save(member);

            SendResetPasswordEmail(member.Email, member.Username, key.ToString());

            TempData["ForgotPasswordIsSuccessful"] = true;
            return RedirectToCurrentUmbracoPage();
        }

        /// <summary>
        /// This version is called from the resend email page. It sends them a new verification email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendVerificationLink([Bind(Prefix = "sendVerificationLinkModel")] SendVerificationLinkModel model)
        {
            // If the user model is valid and the user exists
            if (!ModelState.IsValid)
            {
                // Return the user to the home page
                return CurrentUmbracoPage();
            }

            // Get a handle on the member
            var member = Services.MemberService.GetByUsername(model.UserName);

            if (member == null)
            {
                // If the user doesn't exists, check the HRI API to see if this is a returning IWS user
                var currentUmbracoPage = InitiateSecurityUpgradeForIwsUser("sendVerificationLinkModel", model.UserName);
                if (currentUmbracoPage != null)
                {
                    return currentUmbracoPage;
                }

                // There is an API error
                ModelState.AddModelError("sendVerificationLinkModel", UsernameDoesNotExist);
                return CurrentUmbracoPage();
            }

            if (member.IsApproved)
            { 
                if (!Roles.IsUserInRole(model.UserName, "Registered"))
                { // User is in process of security upgrade
                    return SendResetPasswordEmailAndRedirectToSecurityUpgradePage(model.UserName);
                }

                TempData["ResendEmailAlreadyVerified"] = true;
                return CurrentUmbracoPage();
            }

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

            var protocol = Request.IsSecureConnection ? "https" : "http";
            // Build a dictionary for all the dynamic text in the email template
            var dynamicText = new Dictionary<string, string>
            {
                { "<%FirstName%>", member.GetValue("firstName").ToString() },
                { "<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString() },
                {
                    "<%VerificationUrl%>",
                    protocol + "://" + Request.Url.Host + ":" + Request.Url.Port +
                        "/umbraco/Surface/MembersSurface/ActivateUser?username=" + model.UserName + "&guid=" +
                        key
                }
            };

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
            return Redirect(!string.IsNullOrEmpty(model.RedirectUrl) ? model.RedirectUrl : "/");
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