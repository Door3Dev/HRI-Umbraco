using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace HRI.Services
{
    public class UserService
    {
        private readonly IMemberService _memberService;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;

        public UserService()
        {
            _memberService = ApplicationContext.Current.Services.MemberService;
            _contentService = ApplicationContext.Current.Services.ContentService;
            _mediaService = ApplicationContext.Current.Services.MediaService;
        }

        /// <summary>
        /// Save username to cookie file
        /// </summary>
        /// <param name="username">Username</param>
        public void RememberUsername(string username)
        {
            var userCookie = new HttpCookie("RememberUsername", username);
            userCookie.Expires.AddDays(60);
            HttpContext.Current.Response.SetCookie(userCookie);
        }

        /// <summary>
        /// Get saved username from cookie file
        /// </summary>
        /// <returns>Username</returns>
        public string GetRememberedUsername()
        {
            return HttpContext.Current.Request.Cookies["RememberUsername"] != null 
                    ? HttpContext.Current.Request.Cookies.Get("RememberUsername").Value : string.Empty;
        }

        /// <summary>
        /// Add user to the role
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="role">User role</param>
        public void AddToRole(string username, string role)
        {
            // Check should be always performed or exception will be fired
            if (!Roles.IsUserInRole(username, role))
                Roles.AddUserToRole(username, "role");
        }

        /// <summary>
        /// Remove user from the role
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="role">User role</param>
        public void RemoveFromRole(string username, string role)
        {
            // Check should be always performed or exception will be fired
            if (Roles.IsUserInRole(username, role))
                Roles.RemoveUserFromRole(username, role);
        }

        public bool SendVerificationEmail(string userName)
        {
            if (_memberService.GetByUsername(userName) != null)
            {
                // Get a handle on the member
                var member = _memberService.GetByUsername(userName);
                // Create a random Guid
                var key = Guid.NewGuid();
                // Update the user's Guid field
                member.SetValue("guid", key.ToString());
                // Save the updated information
                _memberService.Save(member);

                // Get ahold of the root/home node
                var root = _contentService.GetRootContent().First();
                // Get the Verification Email Template ID
                var emailTemplateId = root.GetValue<int>("verificationEmailTemplate");

                // Build a dictionary for all the dynamic text in the email template
                var dynamicText = new Dictionary<string, string>
                {
                    {"<%FirstName%>", member.GetValue<string>("firstName")},
                    {"<%PhoneNumber%>", root.GetValue<string>("phoneNumber")},
                    {
                        "<%VerificationUrl%>",
                        root.GetValue<string>("hostUrl") +
                        "/umbraco/Surface/MembersSurface/ActivateUser?id=" + member.Id + "&guid=" + key
                    }
                };


                SendEmail(member.Email, "Health Republic Insurance - Member Verification Link",
                                        BuildEmail(emailTemplateId, dynamicText));
                return true;
            }

            return false;
        }


        /// <summary>
        /// Build an email message from a template.
        /// </summary>
        /// <param name="emailTemplateId">The template file located in the ~/EmailTemplates folder</param>
        /// <param name="values">A dictionary that contains the dynamic placeholder as a key, and has the text to insert as the value. (ex item["<%UserName%>", model.UserName])</param>
        /// <returns>A string representation of the email with all the dynamic text replaced by the provided values</returns>
        protected string BuildEmail(int emailTemplateId, IDictionary<string, string> values)
        {
            // Create a string to hold the email text
            string emailMessage;
            // Get an instance of the template
            var mediaItem = _mediaService.GetById(emailTemplateId);
            // Get the path to the template
            var path = HostingEnvironment.MapPath(mediaItem.Properties["umbracoFile"].Value as string);
            // Open a Stream Reader to read in all the text from the template
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                // Read all the text into the emailMessage string
                emailMessage = sr.ReadToEnd();
                // Close the stream
                sr.Close();
            }
            // For each dynamic item in the template
            foreach (KeyValuePair<string, string> dynamicTextItem in values)
            {
                // Replace the dynamic item with the values member info
                emailMessage = emailMessage.Replace(dynamicTextItem.Key, dynamicTextItem.Value);
            }
            // Return the modfied email template string
            return emailMessage;
        }

        protected void SendEmail(string email, string title, string content, string fromEmail = null)
        {
            // Get ahold of the root/home node
            var root = _contentService.GetRootContent().First();
            // Get the SMTP server
            var smtpServer = root.GetValue<string>("smtpServer");
            // Get the SMTP port
            var smtpPort = Convert.ToInt32(root.GetValue<string>("smtpPort"));
            // Get the SMTP User Name
            var exchangeAccountUserName = root.GetValue<string>("exchangeAccountUserName");
            // Get the SMTP Password
            var exchangeAccountPassword = root.GetValue<string>("exchangeAccountPassword");
            // Get the SMTP email account
            var smtpEmail = fromEmail ?? root.GetValue<string>("smtpEmailAddress");

            // Create a message
            var message = new MailMessage(smtpEmail, email, title, content);

            // Create an SMTP client object and send the message with it
            var smtp = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(exchangeAccountUserName, exchangeAccountPassword)
            };
            // Try to send the message
            smtp.Send(message);
        }
    }
}