using HRI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class EmailSurfaceController : SurfaceController
    {
        /// <summary>
        /// Build an email message from a template.
        /// </summary>
        /// <param name="template">The template file located in the ~/EmailTemplates folder</param>
        /// <param name="values">A dictionary that contains the dynamic placeholder as a key, and has the text to insert as the value. (ex item["<%UserName%>", model.UserName])</param>
        /// <returns>A string representation of the email with all the dynamic text replaced by the provided values</returns>
        private string BuildEmail(string template, IDictionary<string, string> values)
        {
            // Create a string to hold the email text
            string emailMessage;
            // Get the id of the template set in Umbraco
            // TO-DO: make property name dynamic
            var emailtemplateid = CurrentPage.GetProperty(template).Value;
            // Get an instance of the template
            var mediaItem = Services.MediaService.GetById((int)emailtemplateid);
            // Get the path to the template
            string path = Server.MapPath(mediaItem.Properties["umbracoFile"].Value as string);
            // Open a Stream Reader to read in all the text from the template
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                // Read all the text into the emailMessage string
                emailMessage = sr.ReadToEnd();
                // Close the stream
                sr.Close();
            }
            // For each dynamic item in the template
            foreach(KeyValuePair<string,string> dynamicTextItem in values)
            {
                // Replace the dynamic item with the values member info
                emailMessage = emailMessage.Replace(dynamicTextItem.Key, dynamicTextItem.Value);
            }
            // Return the modfied email template string
            return emailMessage;
        }

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
                // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
                IPublishedContent root = Umbraco.TypedContentAtRoot().First();
                // Build a model from the node
                RenderModel rootNodeModel = new RenderModel(root);

                // Get the contact us email value
                string sendTo = rootNodeModel.Content.GetProperty("contactUsEmail").Value.ToString();
                // Get the SMTP server
                string smtpServer = rootNodeModel.Content.GetProperty("smtpServer").Value.ToString();
                // Get the SMTP email account
                string email = rootNodeModel.Content.GetProperty("smtpEmailAddress").Value.ToString();
                // Get the SMTP email password
                string password = rootNodeModel.Content.GetProperty("smtpEmailPassword").Value.ToString();

                // Create a message
                MailMessage message = new MailMessage(model.Email,
                                                      sendTo,
                                                      model.MessageTypes.SelectedValue.ToString(),
                                                      model.Message);

                // Create an SMTP client object and send the message with it
                SmtpClient smtp = new SmtpClient(smtpServer, 587);
                smtp.Credentials = new NetworkCredential(email, password);
                smtp.EnableSsl = true;
                smtp.Send(message);

                // Set the sucess flag to true and post back to the same page
                TempData["IsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex) // If the message failed to send
            {
                // Set the success flag to false and post back to the same page
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ForgotUserName(ForgotUserNameViewModel model)
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
                    // Build a model from the node
                    RenderModel rootNodeModel = new RenderModel(root);

                    // Get the SMTP server
                    string smtpServer = rootNodeModel.Content.GetProperty("smtpServer").Value.ToString();
                    // Get the SMTP email account
                    string smtpEmail = rootNodeModel.Content.GetProperty("smtpEmailAddress").Value.ToString();
                    // Get the SMTP email password
                    string password = rootNodeModel.Content.GetProperty("smtpEmailPassword").Value.ToString();
                    
                    // Build a dictionary for all teh dynamic text in the email template
                    Dictionary<string, string> dynamicText = new Dictionary<string,string>();
                    dynamicText.Add("<%FirstName%>", member.GetValue("firstName").ToString());
                    dynamicText.Add("<%UserName%>", member.Username);
                    dynamicText.Add("<%PhoneNumber%>", rootNodeModel.Content.GetProperty("phoneNumber").Value.ToString());

                    // Create a message
                    MailMessage message = new MailMessage(smtpEmail,
                                                          member.Email,
                                                          "Health Republic Insurance - UserName Recovery",
                                                          BuildEmail("forgotUserNameTemplate", dynamicText));

                    // Create an SMTP client object and send the message with it
                    SmtpClient smtp = new SmtpClient(smtpServer, 587);
                    smtp.Credentials = new NetworkCredential(smtpEmail, password);
                    smtp.EnableSsl = true;
                    smtp.Send(message);

                    // Set the sucess flag to true and post back to the same page
                    TempData["IsSuccessful"] = true;
                    return RedirectToCurrentUmbracoPage();
                }
                else // The email has no member associated with it
                {
                    // Set the success flag to false and post back to the same page
                    TempData["IsSuccessful"] = false;
                    TempData["EmailNotFound"] = true;
                    return RedirectToCurrentUmbracoPage();
                }
            }
            catch(Exception ex)
            {
                // Set the success flag to false and post back to the same page
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
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

                // Send an email to the user
                // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
                IPublishedContent root = Umbraco.TypedContentAtRoot().First();
                // Build a model from the node
                RenderModel rootNodeModel = new RenderModel(root);

                // Get the SMTP server
                string smtpServer = rootNodeModel.Content.GetProperty("smtpServer").Value.ToString();
                // Get the SMTP email account
                string smtpEmail = rootNodeModel.Content.GetProperty("smtpEmailAddress").Value.ToString();
                // Get the SMTP email password
                string password = rootNodeModel.Content.GetProperty("smtpEmailPassword").Value.ToString();

                // Build a dictionary for all teh dynamic text in the email template
                Dictionary<string, string> dynamicText = new Dictionary<string, string>();
                dynamicText.Add("<%FirstName%>", member.GetValue("firstName").ToString());
                dynamicText.Add("<%PhoneNumber%>", rootNodeModel.Content.GetProperty("phoneNumber").Value.ToString());
                dynamicText.Add("<%ResetPasswordLink%>", "http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/Surface/MembersSurface/ResetPassword?username=" + model.UserName + "&guid=" + key.ToString());

                // Create a message
                MailMessage message = new MailMessage(smtpEmail,
                                                      member.Email,
                                                      "Health Republic Insurance - Password Reset",
                                                      BuildEmail("forgotPasswordTemplate", dynamicText));

                // Create an SMTP client object and send the message with it
                SmtpClient smtp = new SmtpClient(smtpServer, 587);
                smtp.Credentials = new NetworkCredential(smtpEmail, password);
                smtp.EnableSsl = true;
                smtp.Send(message);

                TempData["IsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            else
            {
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }


        [HttpPost]
        public ActionResult SendVerificationLink(SendVerificationLinkViewModel model)//string username, string redirectUrl)
        {            
            if (ModelState.IsValid && Services.MemberService.GetByUsername(model.UserName) != null)
            {  
                // Get a handle on the member
                var member = Services.MemberService.GetByUsername(model.UserName);
                
                MembershipUser member2 = System.Web.Security.Membership.GetUser(model.UserName);

                // Send an email to the user
                // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
                IPublishedContent root = Umbraco.TypedContentAtRoot().First();
                // Build a model from the node
                RenderModel rootNodeModel = new RenderModel(root);

                // Get the SMTP server
                string smtpServer = "smtp.live.com";//rootNodeModel.Content.GetProperty("smtpServer").Value.ToString();
                // Get the SMTP email account
                string smtpEmail = "mattwood2855@hotmail.com";// rootNodeModel.Content.GetProperty("smtpEmailAddress").Value.ToString();
                // Get the SMTP email password
                string smtpPassword = "An03ticLLC";// rootNodeModel.Content.GetProperty("smtpEmailPassword").Value.ToString();

                // Build a dictionary for all the dynamic text in the email template
                Dictionary<string, string> dynamicText = new Dictionary<string, string>();
                dynamicText.Add("<%FirstName%>", member.GetValue("firstName").ToString());
                dynamicText.Add("<%PhoneNumber%>", rootNodeModel.Content.GetProperty("phoneNumber").Value.ToString());
                dynamicText.Add("<%TemporaryPassword%>", member2.ResetPassword());            

                // Create a message
                MailMessage message = new MailMessage(smtpEmail,
                                                      "mattwood2855@gmail.com",
                                                      "Health Republic Insurance - Password Reset",
                                                      BuildEmail("securityUpgradeEmailTemplate", dynamicText));

                // Create an SMTP client object and send the message with it
                SmtpClient smtp = new SmtpClient(smtpServer, 587);
                smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
                smtp.EnableSsl = true;
                smtp.Send(message);

                TempData["IsSuccessful"] = true;

                if (model.RedirectUrl != "" && model.RedirectUrl != null)
                    return Redirect(model.RedirectUrl);
                else
                    return Redirect("/");
            }
            else
            {
                TempData["IsSuccessful"] = false;
                return Redirect("/");
            }
        }
    }
}