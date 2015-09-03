using System.Web.Security;
using HRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using HRI.ViewModels;
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
                // This code will create a Document of type Contact Submission and add it to the Contact Us Submissions list in the Umbraco Back end
                // This is done in case there are any issues with the emailer.
                // Get Submissions List Node ID
                var submissionsNodeId = Umbraco.TypedContentAtRoot().First(_ => _.Name == "Contact Us Submissions").Id;
                // Create a new ContactSubmission document and title it the users name
                IContent doc = ApplicationContext.Services.ContentService.CreateContent(model.FirstName + " " + model.LastName, submissionsNodeId, "ContactSubmission");
                // Populate all the data                
                doc.Properties["messageTopic"].Value = model.MessageType;
                doc.Properties["firstName"].Value = model.FirstName;
                doc.Properties["lastName"].Value = model.LastName;
                doc.Properties["email"].Value = model.Email;
                doc.Properties["phoneNumber"].Value = model.PhoneNumber;
                doc.Properties["message"].Value = model.Message;  
                // If this is a logged in user
                if (User.Identity.IsAuthenticated) doc.Properties["memberId"].Value = model.MemberId;
                // Save (but do not publish) the contact submision
                ApplicationContext.Services.ContentService.Save(doc);

                string mailData = CurrentPage.GetPropertyValue<string>("categoriesAndEmails");
                IDictionary<string, IEnumerable<string>> categoriesAndEmails = ContactFormViewModel.GetCategoriesAndEmails(mailData);
                IEnumerable<string> emails = categoriesAndEmails[model.MessageType];

                const string na = "N/A";
                // Build a dictionary for all the dynamic text in the email template
                var dynamicText = new Dictionary<string, string>
                    {
                        {"<%MemberId%>", model.MemberId ?? na},
                        {"<%FirstName%>", model.FirstName},
                        {"<%LastName%>", model.LastName},
                        {"<%YNumber%>", model.YNumber ?? na},
                        {"<%Username%>", model.Username ?? na},
                        {"<%Email%>", model.Email},
                        {"<%Phone%>", model.PhoneNumber},
                        {"<%MessageBody%>", model.Message}
                    };
                // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
                IPublishedContent root = Umbraco.TypedContentAtRoot().First();

                //Get the Contact Us Email Template ID
                object emailTemplateId = null;
                string smtpEmail = null;
                string template = CurrentPage.GetPropertyValue<string>("emailTemplate");
                if (template == "Member")
                {
                    emailTemplateId = root.GetProperty("memberContactUsTemplate").Value;
                    smtpEmail = root.GetProperty("smtpEmailAddress").Value.ToString();
                }
                if (template == "Provider")
                {
                    emailTemplateId = root.GetProperty("providerContactUsTemplate").Value;
                    smtpEmail = root.GetProperty("providerSmtpEmailAddress").Value.ToString();
                }
                if (template == "Broker")
                {
                    emailTemplateId = root.GetProperty("brokerContactUsTemplate").Value;
                    smtpEmail = root.GetProperty("brokerSmtpEmailAddress").Value.ToString();
                }
                
                foreach (string email in emails)
                {
                    try
                    {
                        SendEmail(email, model.MessageType, BuildEmail((int)emailTemplateId, dynamicText), smtpEmail); 
                    }
                    catch (Exception ex)
                    {
                        // Create an error message with sufficient info to contact the user
                        string additionalInfo = model.FirstName + " " + model.LastName + " could not send a contact us email request. Please contact at " + model.Email;
                        // Add the error message to the log4net output
                        log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                        // Log the error
                        logger.Error("Unable to complete Contact Us submission due to SMTP error", ex);
                        logger.Error("Template: " + (template != null ? template : "error") + " - " + "templateID: " + (emailTemplateId != null ? emailTemplateId.ToString() : "error") + " : " + (smtpEmail != null ? smtpEmail : "error"));
                        // Set the sucess flag to true and post back to the same page
                        TempData["IsSuccessful"] = false;
                        return RedirectToCurrentUmbracoPage();
                    }
                }

                // Set the sucess flag to true and post back to the same page
                TempData["IsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex) // If the message failed to send
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = model.FirstName + " " + model.LastName + " could not send a contact us email request. Please contact at " + model.Email;
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                // Log the error
                logger.Error("Unable to complete Contact Us submission", ex);
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
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "A user was trying to retrieve a username for the email " + model.Email + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                // Log the error
                logger.Error("User unable to retrieve forgotten user name.", ex);

                ModelState.AddModelError("forgotUserNameViewModel", ex.Message + "\n" + ex.InnerException.Message + "\n");
                // Set the success flag to false and post back to the same page
                TempData["ForgotUsernameIsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            try
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

                if (member.IsLockedOut)
                    Membership.Provider.UnlockUser(model.UserName);

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

                SendResetPasswordEmail(member);

                TempData["ForgotPasswordIsSuccessful"] = true;
                TempData["memberEmail"] = member.Email;
                return RedirectToCurrentUmbracoPage();
            }
            catch(Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "A user was trying to retrieve a password for the username " + model.UserName + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                // Log the error
                logger.Error("User unable to retrieve forgotten user name.", ex);

                TempData["ForgotPasswordIsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        /// <summary>
        /// This version is called from the resend email page. It sends them a new verification email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendVerificationLink([Bind(Prefix = "sendVerificationLinkModel")] SendVerificationLinkModel model)
        {
            try
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

                // Get the id of the user


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
                        "/umbraco/Surface/MembersSurface/ActivateUser?id=" + member.Id + "&guid=" +
                        key
                }
            };

                // Try to send the message
                try
                {
                    SendEmail(member.Email, "Health Republic Insurance - Member Verification Link",
                        BuildEmail((int)emailTemplateId, dynamicText));
                }
                catch (SmtpException ex)
                {
                    // Create an error message with sufficient info to contact the user
                    string additionalInfo = "Failed to send verification link to user" + model.UserName+ " due to a the SMTP server failing.";
                    // Add the error message to the log4net output
                    log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                    // Log the error
                    logger.Error("Unable to send verification link.", ex);

                    //don't add a field level error, just model level
                    ModelState.AddModelError("sendVerificationLinkModel", ex.Message + "\n" + ex.InnerException.Message + "\n");
                    return CurrentUmbracoPage();
                }

                // Mark this method as successful for the next page
                TempData["IsSuccessful"] = true;

                // If there is a redirect url
                return Redirect(!string.IsNullOrEmpty(model.RedirectUrl) ? model.RedirectUrl : "/");
            }
            catch(Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "Could not send a verification link to user " + model.UserName + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                // Log the error
                logger.Error("Unable to send verification link.", ex);

                TempData["IsSuccessful"] = false;
                return CurrentUmbracoPage();
            }
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
                var key = Guid.NewGuid();
                // Update the user's Guid field
                member.SetValue("guid", key.ToString());
                // Save the updated information
                Services.MemberService.Save(member);

                // Get ahold of the root/home node
                IPublishedContent root = Umbraco.ContentAtRoot().First();
                // Get the Verification Email Template ID
                var emailTemplateId = root.GetProperty("verificationEmailTemplate").Value;                

                // Build a dictionary for all the dynamic text in the email template
                var dynamicText = new Dictionary<string, string>
                {
                    {"<%FirstName%>", member.GetValue("firstName").ToString()},
                    {"<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString()},
                    {
                        "<%VerificationUrl%>",
                        root.GetProperty("HostUrl").Value.ToString() +
                        "/umbraco/Surface/MembersSurface/ActivateUser?id=" + member.Id + "&guid=" +
                        key.ToString()
                    }
                };

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