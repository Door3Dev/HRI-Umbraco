using System.Diagnostics;
using System.Xml;
using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using CoverMyMeds.SAML.Library;
using HRI.Helpers;
using HRI.Models;
using HRI.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;
using Umbraco.Core.Models;
using log4net;

namespace HRI.Controllers
{
    public class MembersSurfaceController : SurfaceController
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(MembersSurfaceController));

        private const string IncorrectPassword = "The password you entered does not match our records, please try again.";

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                Session.Clear();
                FormsAuthentication.SignOut();

                if (Request.Browser.IsMobileDevice)
                    return Redirect("/for-members/login");

                return Redirect("/");
            }
            catch (Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "Error when user " + User.Identity.Name + " attempted to log out.";
                // Add the error message to the log4net output
                GlobalContext.Properties["additionalInfo"] = additionalInfo;
                // Log the error
                logger.Error("Log out error", ex);
                return Redirect("/");
            }
        }

        private bool IsInInitialEnrollmentPeriod()
        {
            var startYear = DateTime.Now.Month >= 4 ? DateTime.Now.Year : DateTime.Now.Year - 1;
            var start = new DateTime(startYear, 11, 15);
            var end = new DateTime(startYear + 1, 3, 31);
            return DateTime.Now.Date >= start && DateTime.Now.Date <= end;
        }

        public ActionResult SingleSignOn(string attributes, string targetUrl, string partnerSP)
        {
            try
            {
                // Initiate single sign-on to the service provider (IdP-initiated SSO)]
                // by sending a SAML response containing a SAML assertion to the SP.

                // get the member id (was IWS number) from the database 
                var member = Services.MemberService.GetByUsername(User.Identity.Name);
                Trace.TraceInformation(DateTime.Now.ToShortTimeString() + ":" + string.Format("---------------------USER '{0}' initiated the SSO---------------------", member.Username));

                // Create a dictionary of attributes to add to the SAML assertion
                var attribs = new Dictionary<string, string>();


                /////////////////////////////////////////////////////////////////////////
                // SAML Parameter Configurations
                /////////////////////////////////////////////////////////////////////////

                // Attributes for StatDoctors
                if (partnerSP == "StatDoctors")
                {

                    string AccountUniqueContactId = member.GetValue("yNumber").ToString();

                    string AccountFamilyId = member.GetValue("yNumber").ToString();
                    if (AccountFamilyId.Length > 7)
                        AccountFamilyId = AccountFamilyId.Substring(0, 7);

                    string FamilyDependentId = member.GetValue("yNumber").ToString();
                    if (FamilyDependentId.Length > 7)
                        FamilyDependentId = FamilyDependentId.Substring(7, 2);
                    {
                        // Create attribute list an populate with needed data
                        var attrib = new Dictionary<string, string>
                        {
                 
                            {"AccountUniqueContactId", AccountUniqueContactId},
                            {"AccountFamilyId", AccountFamilyId},
                            {"FamilyDependentId", FamilyDependentId},
                            {"PartnerId", "AC4134"},
                            {"PartnerAccountId", ""},
                           {"ReturnUrl", ""}

                        };


                        // Send an IdP initiated SAML assertion
                        SAMLIdentityProvider.InitiateSSO(
                            Response,
                            member.GetValue("yNumber").ToString(),
                            attrib,
                            "",
                            partnerSP);
                    }
                }

                // Attributes for US Script
                if (partnerSP == "USScript")
                {
                    string yNumber = member.GetValue("yNumber").ToString();
                    if (yNumber.Length > 7)
                        yNumber = yNumber.Substring(0, 7);

                    var samlAttributes = new Dictionary<string, string>
                    {
                        {"urn:uss:saml:attrib::id", yNumber},
                        {"urn:uss:saml:attrib::firstname", member.GetValue("msFirstName").ToString()},
                        {"urn:uss:saml:attrib::lastname", member.GetValue("msLastName").ToString()},
                        {"urn:uss:saml:attrib::groupid", member.GetValue("groupId").ToString()},
                        {"urn:uss:saml:attrib::dateofbirth", Convert.ToDateTime(member.GetValue("birthday")).ToString("yyyy-MM-dd")},
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
                        {"member:first_name", member.GetValue("msFirstName").ToString()},
                        {"member:last_name", member.GetValue("msLastName").ToString()},
                        {"member:product", member.GetValue("healthPlanName").ToString()}
                    };

                    SAML20Assertion.GuideSSO(Response, partnerSP, member.GetValue("yNumber").ToString(), samlAttributes);
                }

                // Attributes for HealthX
                if (partnerSP == "https://secure.healthx.com/PublicService/SSO/AutoLogin.aspx"
                    || partnerSP == "https://secure.healthx.com/PublicService/SSO/AutoLogin.aspx?mobile=1")
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
                            "MemberLastName", "xs:string", member.GetValue("msLastName").ToString().ToUpper()),
                        new SAMLAttribute("MemberFirstName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                            "MemberFirstName", "xs:string", member.GetValue("msFirstName").ToString().ToUpper()),
                        new SAMLAttribute("UserLastName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri", "UserLastName",
                            "xs:string", member.GetValue("msLastName").ToString().ToUpper()),
                        new SAMLAttribute("UserFirstName", "urn:oasis:names:tc:SAML:2.0:attrname-format:uri",
                            "UserFirstName", "xs:string", member.GetValue("msFirstName").ToString().ToUpper())
                    };

                    // Nest a node named ServiceId in the RedirectInfo attribute 
                    // Add a serializer to allow the nesting of the serviceid attribute without it being url encoded    
                    if (!AttributeType.IsAttributeValueSerializerRegistered("RedirectInfo", null))
                        AttributeType.RegisterAttributeValueSerializer("RedirectInfo", null, new XmlAttributeValueSerializer());

                    // Add Redirect Info xml
                    var xmlRedirectInfo = new XmlDocument { PreserveWhitespace = true };
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

                    // Replace "initialEnrollment" with "specialEnrollmentSelect" if outside of 11/15-3/31
                    if (targetUrl.Contains("initialEnrollment") && !IsInInitialEnrollmentPeriod())
                    {
                        targetUrl = targetUrl.Replace("initialEnrollment", "specialEnrollmentSelect");
                    }

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
            catch (Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "SSO Error for user " + User.Identity.Name + ". Partner: " + partnerSP + ". TargetUrl: " + targetUrl + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                // Log the error
                logger.Error("Unable to use SSO", ex);

                return new EmptyResult();
            }
        }

        [RequireRouteValues(new[] { "id", "guid" })]
        public ActionResult ActivateUser(int id, string guid)
        {
            var protocol = Request.IsSecureConnection ? "https" : "http";
            // String to api call to register the current user

            var json = new JObject();
            try
            {
                // validate guid passed
                //IMember member = Services.MemberService.GetByUsername(userName);
                IMember member = Services.MemberService.GetById(id);
                string userName = member.Username;
                string registerApiUrl = protocol + "://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/api/HriApi/RegisterUser?userName=" + userName;
                string userGuid = member.GetValue("guid").ToString();
                if (!string.Equals(userGuid, guid, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(string.Format("Guid '{0}' does not match user '{1}'", guid, userName));

                // Variable to hold status of registering user against HRI API
                bool regSuccess;
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
                    if (!regSuccess)
                        logger.ErrorFormat("User '{0}' Activation API error: {1}", json, userName);
                }
                // If a success
                if (regSuccess)
                {
                    member = Services.MemberService.GetByUsername(userName);
                    // Set the user to be approved
                    member.IsApproved = true;
                    // Add the registered role to the user
                    Roles.AddUserToRole(userName, "Registered");
                    // Add the enrolled role to the user, if "enrollmentpageafterlogin" != 1
                    if (member.GetValue<string>("enrollmentpageafterlogin") != "1")
                    {
                        Roles.AddUserToRole(userName, "Enrolled");
                    }
                    // Save the member
                    Services.MemberService.Save(member);
                    // Send the user to the login page
                    TempData["RegistrationResult"] = RegistrationNotificationType.Success;
                    return Redirect("/for-members/login");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            TempData["RegistrationResult"] = RegistrationNotificationType.Error;
            return Redirect("/for-members/login");
        }

        [RequireRouteValues(new[] { "username", "guid" }), ActionName("ActivateUser")]
        public ActionResult DeprecatedActivateUser(string username, string guid)
        {
            TempData["RegistrationResult"] = RegistrationNotificationType.DeprecatedUrlError;
            return Redirect("/for-members/login");
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
            var member = Services.MemberService.GetByUsername(user.UserName);

            if (!Membership.ValidateUser(user.UserName, model.Password))
            {
                ModelState.AddModelError("changeEmailViewModel", IncorrectPassword);
            }

            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            try
            {
                // Update Email with API
                var hriService = new HriApiService();
                var apiUpdateSuccess = hriService.UpdateUserEmail(member, model.Email);
                // CMS should be updated only if API returns success
                if (apiUpdateSuccess)
                {
                    // Set the user's email address to the new supplied email address.
                    user.Email = model.Email;
                    // Update the User profile in the database
                    Membership.UpdateUser(user);
                }

                // Set the success flag to true
                TempData["IsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ChangePassword([Bind(Prefix = "changePasswordViewModel")] ChangePasswordViewModel model)
        {
            try
            {
                var user = Membership.GetUser();
                if (user == null || !Membership.ValidateUser(user.UserName, model.OldPassword))
                {
                    ModelState.AddModelError("changePasswordViewModel", IncorrectPassword);
                }
                // Verify that username and password arent the same.
                if (user.UserName == model.NewPassword)
                {
                    ModelState.AddModelError("changePasswordViewModel.NewPassword", "Password cannot be the same as Username");
                }

                if (string.Compare(model.OldPassword, model.NewPassword, StringComparison.Ordinal) == 0)
                {
                    ModelState.AddModelError("changePasswordViewModel.NewPassword", "Your new password cannot be the same as your current password.");
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
                    if (!Roles.IsUserInRole(user.UserName, "Registered"))
                        Roles.AddUserToRole(user.UserName, "Registered"); // This is needed to end security upgrade process
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
            catch (Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "User " + User.Identity.Name + " was unable to change their password.";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                logger.Error(ex);
                return CurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ChangeUserName(ChangeUserNameViewModel model)
        {
            try
            {
                // TO-DO: Either extend the membership provider to support username changes
                // or create a new user and copy all the data as per 
                // http://stackoverflow.com/questions/1001491/is-it-possible-to-change-the-username-with-the-membership-api
                //
                var user = Membership.GetUser();
                //user.UserName = model.UserName;
                // Update the User profile in the database
                Membership.UpdateUser((System.Web.Security.MembershipUser)user);
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(int id, string guid)
        {
            try
            {
                // Verify the member exists
                // Verify the user provided the correct guid
                var member = Services.MemberService.GetById(id);
                if (member == null || member.GetValue<string>("guid") != guid)
                    return Redirect("/");

                SetUserNameAndGuide(member.Username, guid);
                return Redirect("/for-members/reset-password/");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Redirect("/for-members/reset-password/");
            }
        }

        [HttpPost]
        public ActionResult ResetPassword([Bind(Prefix = "resetPasswordViewModel")] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetUserNameAndGuide(model.UserName, model.Guid);
                return CurrentUmbracoPage();
            }

            try
            {
                var member = Membership.GetUser(model.UserName);
                if (member.IsLockedOut)
                {
                    Membership.Provider.UnlockUser(model.UserName);
                    member = Membership.GetUser(model.UserName);
                }
                var tempPassword = member.ResetPassword();
                member.ChangePassword(tempPassword, model.NewPassword);
                Membership.UpdateUser(member);
                if (!Roles.IsUserInRole(model.UserName, "Registered"))
                    Roles.AddUserToRole(model.UserName, "Registered"); // This is needed to end security upgrade process

                TempData["ResetPasswordIsSuccessful"] = true;
                return RedirectToCurrentUmbracoPage();
            }
            catch (MembershipPasswordException)
            {
                ModelState.AddModelError(
                    "resetPasswordViewModel.NewPassword",
                    RegisterSurfaceController.PasswordNotStrongEnough);

                SetUserNameAndGuide(model.UserName, model.Guid);
                return CurrentUmbracoPage();
            }
        }

        private void SetUserNameAndGuide(string userName, string guid)
        {
            // Set the username and guid
            TempData["username"] = userName;
            TempData["guid"] = guid;
        }
    }
}