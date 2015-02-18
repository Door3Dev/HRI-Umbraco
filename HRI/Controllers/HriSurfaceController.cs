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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using log4net;

namespace HRI.Controllers
{
    /// <summary>
    /// Base Surface Controller that has helper methods
    /// </summary>
    public class HriSufraceController : SurfaceController
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(HriSufraceController));

        /// <summary>
        /// Build an email message from a template.
        /// </summary>
        /// <param name="template">The template file located in the ~/EmailTemplates folder</param>
        /// <param name="values">A dictionary that contains the dynamic placeholder as a key, and has the text to insert as the value. (ex item["<%UserName%>", model.UserName])</param>
        /// <returns>A string representation of the email with all the dynamic text replaced by the provided values</returns>
        protected string BuildEmail(int emailTemplateId, IDictionary<string, string> values)
        {
            // Create a string to hold the email text
            string emailMessage;
            // Get an instance of the template
            var mediaItem = Services.MediaService.GetById(emailTemplateId);
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
            IPublishedContent root = Umbraco.ContentAtRoot().First();
            // Get the SMTP server
            string smtpServer = root.GetProperty("smtpServer").Value.ToString();
            // Get the SMTP port
            int smtpPort = Convert.ToInt32(root.GetProperty("smtpPort").Value);
            // Get the SMTP User Name
            string exchangeAccountUserName = root.GetProperty("exchangeAccountUserName").Value.ToString();
            // Get the SMTP Password
            string exchangeAccountPassword = root.GetProperty("exchangeAccountPassword").Value.ToString();
            // Get the SMTP email account
            string smtpEmail = fromEmail != null ? fromEmail : root.GetProperty("smtpEmailAddress").Value.ToString();

            // Create a message
            MailMessage message = new MailMessage(smtpEmail, email, title, content);

            // Create an SMTP client object and send the message with it
            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new NetworkCredential(exchangeAccountUserName, exchangeAccountPassword);
            // Try to send the message
            smtp.Send(message);
        }

        protected void SendResetPasswordEmail(string email, string username, string guid)
        {
            // Get the Umbraco root node to access dynamic information (phone numbers, emails, ect)
            IPublishedContent root = Umbraco.TypedContentAtRoot().First();

            var protocol = Request.IsSecureConnection ? "https" : "http";
            // Build a dictionary for all the dynamic text in the email template
            var dynamicText = new Dictionary<string, string>
            {
                {"<%FirstName%>", username},
                {"<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString()},
                {
                    "<%ResetPasswordLink%>",
                    protocol + "://" + Request.Url.Host + ":" + Request.Url.Port +
                    "/umbraco/Surface/MembersSurface/ResetPassword?userName=" + username + "&guid=" + guid
                }
            };

            //Get the Verification Email Template ID
            var emailTemplateId = root.GetProperty("resetPasswordEmailTemplate").Value;

            // Send the email with the new password
            SendEmail(email,
                        "Health Republic Insurance - Password Reset Link",
                        BuildEmail((int)emailTemplateId, dynamicText));
        }

        /// <summary>
        /// Make internal HriApi call
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="values">List of values</param>
        /// <returns></returns>
        protected string MakeInternalApiCall(string action, Dictionary<string, string> values)
        {
            // Trust to the certificates during the call
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            // Set the HTTPS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // Exectue a GET against the API
            using (var client = new WebClient())
            {
                // Read the response into a string
                /*var valuesList = values.Select(_ => String.Format("{0}={1}", HttpUtility.UrlEncode(_.Key), HttpUtility.UrlEncode(_.Value)));
                var restUrl = WebConfigurationManager.AppSettings["umbracoRestApiUrl"];
                string url = String.Format("{0}/umbraco/api/HriApi/{1}?{2}", restUrl, action, String.Join("&", valuesList));

                return client.DownloadString(url);*/
                var valuesList = values.Select(_ => String.Format("{0}={1}", HttpUtility.UrlEncode(_.Key), HttpUtility.UrlEncode(_.Value)));
                var protocol = Request.IsSecureConnection ? "https" : "http";
                string url = String.Format("{0}://{1}:{2}/umbraco/api/HriApi/{3}?{4}", protocol, Request.Url.Host,
                    Request.Url.Port, action, String.Join("&", valuesList));
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// Make internal HriApi call
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="values">List of values</param>
        /// <returns>Json object</returns>
        protected JObject MakeInternalApiCallJson(string action, Dictionary<string, string> values)
        {
            var result = MakeInternalApiCall(action, values);
            return result == "null" ? null : JObject.Parse(result);
        }

        protected T MakeInternalApiCall<T>(string action, Dictionary<string, string> values)
        {
            return JsonConvert.DeserializeObject<T>(MakeInternalApiCall(action, values));
        }

        protected ActionResult InitiateSecurityUpgradeForIwsUser(string model, string username)
        {
            var hriUser = MakeInternalApiCallJson("GetRegisteredUserByUsername",
                new Dictionary<string, string> { { "userName", username } });

            // Check if the user exists in IWS database
            if (hriUser == null || hriUser["RegId"] == null)
            {
                return null;
            }

            // Before attempt to create a user need to check the email and login uniqueness
            var existedUserWithEmail = Services.MemberService.GetByEmail(hriUser["EMail"].ToString());
            if (existedUserWithEmail != null)
            {
                ModelState.AddModelError(
                    model,
                    "We cannot log you in with this user name. The email address of the user name you entered is associated with another user name. Please enter a valid user name and try again or contact Member Services for assistance.");

                return CurrentUmbracoPage();
            }

            // Create the registration model
            var registerModel = Members.CreateRegistrationModel();
            // Member Name
            registerModel.Name = hriUser["FirstName"] + " " + hriUser["LastName"];
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
            {
                registerModel.MemberProperties.First(p => p.Alias == "ssn").Value = hriUser["Ssn"].ToString();
            }
            // SSN
            if ((string)hriUser["EbixId"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "ebixId").Value = hriUser["ebixID"].ToString();
            }
            // Email
            if ((string)hriUser["EMail"] != null)
            {
                registerModel.Email = hriUser["EMail"].ToString();
            }
            // Zip Code
            if ((string)hriUser["ZipCode"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "zipCode").Value = hriUser["ZipCode"].ToString();
            }
            // Phone Number
            if ((string)hriUser["PhoneNumber"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "phoneNumber").Value =
                    hriUser["PhoneNumber"].ToString();
            }
            // Y Number
            if ((string)hriUser["MemberId"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "yNumber").Value = hriUser["MemberId"].ToString();
            }
            // Group Id
            if ((string)hriUser["RxGrpId"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "groupId").Value = hriUser["RxGrpId"].ToString();
            }
            // Birthday
            if ((string)hriUser["DOB"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "birthday").Value = hriUser["DOB"].ToString();
            }
            // Plan Id
            if ((string)hriUser["PlanId"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "healthplanid").Value = hriUser["PlanId"].ToString();
            }
            // Plan Name
            if ((string)hriUser["PlanName"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "healthPlanName").Value = hriUser["PlanName"].ToString();
            }
            // Morneau Shepell First Name
            if ((string)hriUser["MSFirstName"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "msFirstName").Value = hriUser["MSFirstName"].ToString();
            }
            // Morneau Shepell Last Name
            if ((string)hriUser["MSLastName"] != null)
            {
                registerModel.MemberProperties.First(p => p.Alias == "msLastName").Value = hriUser["MSLastName"].ToString();
            }

            registerModel.MemberProperties.First(p => p.Alias == "market").Value = hriUser["Market"].ToString();
            registerModel.MemberProperties.First(p => p.Alias == "effectiveYear").Value = hriUser["PlanEffectiveDate"].ToString();
            
            registerModel.Password = Membership.GeneratePassword(12, 4);
            registerModel.LoginOnSuccess = false;
            registerModel.UsernameIsEmail = false;

            // Register the user with automatically
            MembershipCreateStatus status;
            Members.RegisterMember(registerModel, out status, registerModel.LoginOnSuccess);


            
            if ((string)hriUser["DOB"] != null)
            {
                logger.Info("Adding role for '" + username + "'");
                logger.Info(hriUser);
                Roles.AddUserToRole(username, "Enrolled");
            }

            // Force sign out (hack for Umbraco bug that automatically logs user in on registration
            Session.Clear();
            FormsAuthentication.SignOut();

            return SendResetPasswordEmailAndRedirectToSecurityUpgradePage(username);
        }

        protected ActionResult SendResetPasswordEmailAndRedirectToSecurityUpgradePage(string username)
        {
            var member = Services.MemberService.GetByUsername(username);

            // Create a random Guid
            var key = Guid.NewGuid();
            // Update the user's Guid field
            member.SetValue("guid", key.ToString());

            // Distinguishe not activated user from user in process of security upgrade
            member.IsApproved = true;

            Services.MemberService.Save(member);

            // Reset the password and send an email to the user
            SendResetPasswordEmail(member.Email, username, key.ToString());

            return Redirect("/for-members/security-upgrade/");
        }
    }
}