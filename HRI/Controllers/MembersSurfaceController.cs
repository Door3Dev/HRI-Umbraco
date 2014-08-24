using ComponentSpace.SAML2;
using HRI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class MembersSurfaceController : SurfaceController
    {
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
            Dictionary<string, string> attribs = new Dictionary<string, string>();

            /////////////////////////////////////////////////////////////////////////
            // SAML Parameter Configurations
            /////////////////////////////////////////////////////////////////////////

            // Attributes for US Script
            if(partnerSP == "UsScript")
            {
                attribs.Add("clientId", targetUrl);                ;
                attribs.Add("urn:uss:saml:attrib::id", member.GetValue("yNumber").ToString());
                attribs.Add("urn:uss:saml:attrib::firstname", member.GetValue("firstName").ToString());
                attribs.Add("urn:uss:saml:attrib::lastname", member.GetValue("lastName").ToString());
                attribs.Add("email", member.Email);

                
                // Send an IdP initiated SAML assertion
                SAMLIdentityProvider.InitiateSSO(
                    Response,
                    targetUrl, // Use target URL as subject - trying to see how to fix usscript
                    attribs,
                    "",
                    partnerSP);
            }

            // Attributes for MagnaCare
            if (partnerSP == "MagnaCare")
            {                                
                attribs.Add("member:id", member.GetValue("yNumber").ToString());                
                attribs.Add("member:first_name", member.GetValue("firstName").ToString());                
                attribs.Add("member:last_name", member.GetValue("lastName").ToString());
                attribs.Add("member:product", "EPO");

                // Send an IdP initiated SAML assertion
                SAMLIdentityProvider.InitiateSSO(
                    Response,
                    member.GetValue("yNumber").ToString(),
                    attribs,
                    "PRIMARYSELECT",
                    partnerSP);
            }

            // Attributes for HealthX
            if (partnerSP == "https://secure.healthx.com/PublicService/SSO/AutoLogin.aspx")
            {
                //attribs.Add("RedirectInfo", targetUrl);
                //attribs.Add("Version", "1");
                //attribs.Add("RelationshipCode", "18");
                attribs.Add("UserId", member.GetValue("yNumber").ToString());
                attribs.Add("MemberLastName", member.GetValue("lastName").ToString().ToUpper());
                attribs.Add("MemberFirstName", member.GetValue("firstName").ToString().ToUpper());
                //attribs.Add("UserEmailAddress", member.Email);
                //attribs.Add("UserPhoneNumber", member.GetValue("phoneNumber").ToString());

                // Send an IdP initiated SAML assertion
                SAMLIdentityProvider.InitiateSSO(
                    Response,
                    member.GetValue("yNumber").ToString(),
                    attribs,
                    targetUrl,
                    partnerSP);
            }


            // Attributes for Morneau Shapell
            if (partnerSP == "SBCSystems")
            {
                // Replace the template variables in the url
                targetUrl = targetUrl.Replace("<%PLANID%>", member.GetValue("healthPlanId").ToString());
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
            JObject json;
            using(var client = new WebClient())
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
                // Set the user to be approved
                MembershipUser memb = Membership.GetUser(userName);
                memb.IsApproved = true;                      
                // Add the registered role to the user
                System.Web.Security.Roles.AddUserToRole(userName, "Registered");
                // Save the member
                Membership.UpdateUser(memb); 
                // Send the user to the login page
                return Redirect("/for-members/login");
            }
            return Content("There was an error validating your account. Your account may have already been validated. Please try logging in at <a href='/' >the site</a> or contact Health Republic New York.<br><br>" + json["message"]);            
        }

        /// <summary>
        /// HttpPost to change the user's email address
        /// </summary>
        /// <param name="model">Change Email model containing password and email address</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeEmail([Bind(Prefix = "changeEmailViewModel")]ChangeEmailViewModel model)
        {
            try
            {
                // Get the current user
                var user = Membership.GetUser();
                // Verify the password is correct
                if (model.Email == model.Email2)
                {                  
                    if (Membership.ValidateUser(User.Identity.Name, model.Password))
                    {
                        // Set the user's email address to the new supplied email address.
                        user.Email = model.Email;
                        // Update the User profile in the database
                        Membership.UpdateUser((System.Web.Security.MembershipUser)user);
                        // Set the success flag to true
                        TempData["IsSuccessful"] = true;
                        
                    }
                    else // The password was incorrect
                    {
                        ModelState.AddModelError("changeEmailViewModel", "Incorrect Password");
                        // Mark the post as unsuccessful
                        TempData["IsSuccessful"] = false;
                    }                    
                }
                else // Email and confirmation email didnt match
                {
                    // Mark the post as unsuccessful
                    ModelState.AddModelError("changeEmailViewModel", "The email adresses you have entered do not mach");                    
                    //TempData["IsSuccessful"] = false;
                }

                return RedirectToCurrentUmbracoPage();
            }
            catch(Exception ex)
            {
                TempData["IsSuccessful"] = false;
                return RedirectToCurrentUmbracoPage();
            }
        }

        [HttpPost]
        public ActionResult ChangePassword([Bind(Prefix = "changePasswordViewModel")]ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Membership.GetUser();
                TempData["IsSuccessful"] = user.ChangePassword(model.OldPassword, model.NewPassword);
                // Update the User profile in the database
                Membership.UpdateUser((System.Web.Security.MembershipUser)user);                
            }
            return RedirectToCurrentUmbracoPage();
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
        public ActionResult ResetPassword(string username, string guid)
        {
            // Verify the member exists
            var member = Membership.GetUser(username);            
            if(member == null)
                return Redirect("/");

            // Verify the user provided the correct guid
            if (Services.MemberService.GetByUsername(username).GetValue("guid").ToString() != guid.ToString())
            {
                return Redirect("/");
            }

            // Set the username and guid
            TempData["username"] = username;
            TempData["guid"] = guid;
            return Redirect("/for-members/reset-password/");                        
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            var member = Membership.GetUser("test");
            member.ResetPassword();
            return RedirectToCurrentUmbracoPage();
        }
    }
}