using System.Xml;
using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using CoverMyMeds.SAML.Library;
using HRI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class MembersSurfaceController : SurfaceController
    {
        private const string IncorrectPassword = "The password you entered does not match our records, please try again.";

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        public ActionResult SingleSignOn(string attributes, string targetUrl, string partnerSP)
        {
            // Initiate single sign-on to the service provider (IdP-initiated SSO)]
            // by sending a SAML response containing a SAML assertion to the SP.
            
            // get the member id (was IWS number) from the database 
            var member = Services.MemberService.GetByUsername(User.Identity.Name);            

            // Create a dictionary of attributes to add to the SAML assertion
            var attribs = new Dictionary<string, string>();
            
            
            /////////////////////////////////////////////////////////////////////////
            // SAML Parameter Configurations
            /////////////////////////////////////////////////////////////////////////

            // Attributes for US Script
            if(partnerSP == "USScript")
            {
                var samlAttributes = new Dictionary<string, string>
                {
                    {"urn:uss:saml:attrib::id", member.GetValue("yNumber").ToString()},
                    {"urn:uss:saml:attrib::firstname", member.GetValue("firstName").ToString()},
                    {"urn:uss:saml:attrib::lastname", member.GetValue("lastName").ToString()},
                    {"urn:uss:saml:attrib::groupid", member.GetValue("groupId").ToString()},
                    {"urn:uss:saml:attrib::dateofbirth", member.GetValue("birthday").ToString()},
                    {"urn:uss:saml:attrib::email", member.Email}
                };

                PgpSAML20Assertion.GuideSSO(Response, partnerSP, String.Empty, samlAttributes);
            }

            // Attributes for MagnaCare
            if (partnerSP == "MagnaCare")
            {
                var samlAttributes = new Dictionary<string, string>
                {
                    {"member:id", member.GetValue("yNumber").ToString()},
                    {"member:first_name", member.GetValue("firstName").ToString()},
                    {"member:last_name", member.GetValue("lastName").ToString()},
                    {"member:product", member.GetValue("healthPlanName").ToString()}
                };

                SAML20Assertion.GuideSSO(Response, partnerSP, member.GetValue("yNumber").ToString(), samlAttributes);
            }

            // Attributes for HealthX
            if (partnerSP == "HealthX")
            {
                // Create attribute list an populate with needed data
                var attrib = new List<SAMLAttribute>
                {
                    // Version 1 is constant value set by HealthX 
                    new SAMLAttribute("Version", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", "Version",
                        "xs:string", "1"),
                    // This is the site ID and is redundant since it is in the Assertion consumer url. I added this for completeness
                    new SAMLAttribute("ServiceId", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", "ServiceID",
                        "xs:string", "d99bfe58-3896-4eb6-9586-d2f9ae673052"),
                    // This is the service ID and is redundant since it is in the Assertion consumer url. I added this for completeness
                    new SAMLAttribute("SiteId", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", "SiteId", "xs:string",
                        "e6fa832c-fbd3-48c7-860f-e4f04b22bab7"),
                    new SAMLAttribute("RelationshipCode", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                        "RelationshipCode", "xs:string", "18"),
                    new SAMLAttribute("UserId", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", "UserId", "xs:string",
                        member.GetValue("yNumber").ToString().ToUpper()),
                    new SAMLAttribute("MemberLastName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                        "MemberLastName", "xs:string", member.GetValue("lastName").ToString().ToUpper()),
                    new SAMLAttribute("MemberFirstName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                        "MemberFirstName", "xs:string", member.GetValue("firstName").ToString().ToUpper()),
                    new SAMLAttribute("UserLastName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", "UserLastName",
                        "xs:string", member.GetValue("lastName").ToString().ToUpper()),
                    new SAMLAttribute("UserFirstName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                        "UserFirstName", "xs:string", member.GetValue("firstName").ToString().ToUpper())
                };

                // Nest a node named ServiceId in the RedirectInfo attribute 
                // Add a serializer to allow the nesting of the serviceid attribute without it being url encoded    
                if (!AttributeType.IsAttributeValueSerializerRegistered("RedirectInfo", null))
                    AttributeType.RegisterAttributeValueSerializer("RedirectInfo", null, new XmlAttributeValueSerializer());

                // Add Redirect Info xml
                var xmlRedirectInfo = new XmlDocument {PreserveWhitespace = true};
                xmlRedirectInfo.LoadXml(targetUrl);
                var attrRedirectInfo = new SAMLAttribute("RedirectInfo", "urn:oasis:names:tc:SAML:2.0:attrname-format:basic", "RedirectInfo");
                attrRedirectInfo.Values.Add(new AttributeValue(xmlRedirectInfo.DocumentElement));
                attrib.Add(attrRedirectInfo);

                // Send an IdP initiated SAML assertion
                SAMLIdentityProvider.InitiateSSO(
                    Response,
                    member.GetValue("yNumber").ToString(),
                    attrib.ToArray(),
                    "",
                    partnerSP);
            }

            // Attributes for Morneau Shapell
            if (partnerSP == "SBCSystems")
            {
                // Replace the template variables in the url
                if (targetUrl.IndexOf("<%PLANID%>") != -1)
                    targetUrl = targetUrl.Replace("<%PLANID%>", member.GetValue("healthplanid").ToString());
                // Send an IdP initiated SAML assertion
                SAMLIdentityProvider.InitiateSSO(
                    Response,
                    member.GetValue("memberId").ToString(),
                    attribs,
                    targetUrl,
                    partnerSP);
            }

            // Add the response to the ViewBag so we can access it on the front end if we need to
            ViewBag.Response = Response;
            TempData["response"] = Response;
            // Return an empty response since we wait for the SAML consumer to send us the requested page
            return new EmptyResult();
        }        

        public ActionResult ActivateUser(string userName, string guid)
        {
            // Variable to hold status of registering user against HRI API
            bool regSuccess;
            // String to api call to register the current user
            string registerApiUrl = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/api/HriApi/RegisterUser?userName=" + userName;
            JObject json = new JObject();
            try
            {
                using (var client = new WebClient())
                {
                    // Call the register function (Registers user with HRI API)
                    string result = client.DownloadString(registerApiUrl);
                    // Remove the leading and trailing quotes and remove the \ that are used to escape in ToString() from API call
                    result = result.Substring(1, result.Length - 2);
                    result = result.Replace("\\", "");
                    json = JObject.Parse(result);
                    // Determine the result of the registration
                    regSuccess = !Convert.ToBoolean(json["error"]);
                }
                // If a success
                if (regSuccess)
                {
                    var member = Services.MemberService.GetByUsername(userName);
                    if(member.GetValue("memberId").ToString() == null || member.GetValue("memberId").ToString() == "")
                        member.SetValue("memberId", json["RegId"].ToString());
                    if (member.GetValue("yNumber").ToString() == null || member.GetValue("yNumber").ToString() == "")
                        member.SetValue("yNumber", json["MemberId"].ToString());
                    // Set the user to be approved
                    member.IsApproved = true;                    
                    // Add the registered role to the user
                    System.Web.Security.Roles.AddUserToRole(userName, "Registered");
                    // Save the member
                    Services.MemberService.Save(member);                    
                    // Send the user to the login page
                    TempData["IsUserSuccessfullyRegistered"] = true;
                    return Redirect("/for-members/login");
                }
            }
            catch (Exception)
            {
                return Content("There was an error validating your account. Your account may have already been validated. Please try logging in at <a href='/' >the site</a> or contact Health Republic New York.<br><br>" + json["message"]);     
            }
            return Content("There was an error validating your account. Your account may have already been validated. Please try logging in at <a href='/' >the site</a> or contact Health Republic New York.<br><br>" + json["message"]);            
        }

        /// <summary>
        /// HttpPost to change the user's email address
        /// </summary>
        /// <param name="model">Change Email model containing password and email address</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeEmail([Bind(Prefix = "changeEmailViewModel")] ChangeEmailViewModel model)
        {
            var user = Membership.GetUser();
            if (user == null || !Membership.ValidateUser(user.UserName, model.Password))
            {
                ModelState.AddModelError("changeEmailViewModel", IncorrectPassword);
            }

            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            try
            {
                // Set the user's email address to the new supplied email address.
                user.Email = model.Email;
                // Update the User profile in the database
                Membership.UpdateUser(user);
                // Set the success flag to true
                TempData["IsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch(Exception)
            {
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ChangePassword([Bind(Prefix = "changePasswordViewModel")] ChangePasswordViewModel model)
        {
            var user = Membership.GetUser();
            if (user == null || !Membership.ValidateUser(user.UserName, model.OldPassword))
            {
                ModelState.AddModelError("changePasswordViewModel", IncorrectPassword);
            }

            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            try
            {
                TempData["IsSuccessful"] = user.ChangePassword(model.OldPassword, model.NewPassword);
                // Update the User profile in the database
                Membership.UpdateUser(user);
                return RedirectToCurrentUmbracoPage();
            }
            catch (MembershipPasswordException)
            {
                ModelState.AddModelError(
                    "changePasswordViewModel.NewPassword", 
                    RegisterSurfaceController.PasswordNotStrongEnough);

                return CurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ChangeUserName(ChangeUserNameViewModel model)
        {
            // TO-DO: Either extend the membership provider to support username changes
            // or create a new user and copy all the data as per 
            // http://stackoverflow.com/questions/1001491/is-it-possible-to-change-the-username-with-the-membership-api
            //
            var user = Membership.GetUser();
            var user2 = Services.MemberService.GetByUsername("kjfdg");
            //user.UserName = model.UserName;
            // Update the User profile in the database
            Membership.UpdateUser((System.Web.Security.MembershipUser)user);
            return RedirectToCurrentUmbracoPage();
        }

        [HttpGet]
        public ActionResult ResetPassword(string userName, string guid)
        {
            // Verify the member exists
            var member = Membership.GetUser(userName);            
            if(member == null)
                return Redirect("/");

            // Verify the user provided the correct guid
            if (Services.MemberService.GetByUsername(userName).GetValue("guid").ToString() != guid.ToString())
            {
                return Redirect("/");
            }

            // Set the username and guid
            TempData["username"] = userName;
            TempData["guid"] = guid;
            return Redirect("/for-members/reset-password/");                        
        }

        [HttpPost]
        public ActionResult ResetPassword([Bind(Prefix = "resetPasswordViewModel")]ResetPasswordViewModel model)
        {
            var member = Membership.GetUser(model.UserName);
            string tempPassword = member.ResetPassword();
            member.ChangePassword(tempPassword, model.NewPassword);
            Membership.UpdateUser(member);
            TempData["ResetPasswordIsSuccessful"] = true;
            return RedirectToCurrentUmbracoPage();
        }
    }
}