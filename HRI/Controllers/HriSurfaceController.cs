﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    /// <summary>
    /// Base Surface Controller that has helper methods
    /// </summary>
    public class HriSufraceController : SurfaceController
    {
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

        protected void SendEmail(string email, string title, string content)
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
            string smtpEmail = root.GetProperty("smtpEmailAddress").Value.ToString();

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

            // Build a dictionary for all the dynamic text in the email template
            var dynamicText = new Dictionary<string, string>
            {
                {"<%FirstName%>", username},
                {"<%PhoneNumber%>", root.GetProperty("phoneNumber").Value.ToString()},
                {
                    "<%ResetPasswordLink%>",
                    "http://" + Request.Url.Host + ":" + Request.Url.Port +
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
            // Exectue a GET against the API
            using (var client = new WebClient())
            {
                // Read the response into a string
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
    }
}