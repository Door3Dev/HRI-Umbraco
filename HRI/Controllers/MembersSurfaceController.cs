using ComponentSpace.SAML2;
using HRI.Models;
using System;
using System.Collections.Generic;
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

            // Attributes for MagnaCare
            if (partnerSP == "MagnaCare")
            {                
                attribs.Add("Member ID", member.GetValue("yNumber").ToString());
                attribs.Add("member:id", member.GetValue("yNumber").ToString());
                attribs.Add("First Name", member.GetValue("firstName").ToString());
                attribs.Add("member:first_name", member.GetValue("firstName").ToString());
                attribs.Add("Last Name", member.GetValue("lastName").ToString());
                attribs.Add("member:last_name", member.GetValue("lastName").ToString());
                attribs.Add("Product", "EPO");
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
                attribs.Add("RedirectInfo", targetUrl);
                attribs.Add("Version", "1");
                attribs.Add("RelationshipCode", "18");
                attribs.Add("UserId", member.GetValue("memberId").ToString());
                attribs.Add("MemberLastName", member.GetValue("lastName").ToString());
                attribs.Add("MemberFirstName", member.GetValue("firstName").ToString());
                attribs.Add("UserEmailAddress", member.Email);
                attribs.Add("UserPhoneNumber", member.GetValue("phoneNumber").ToString());

                // Send an IdP initiated SAML assertion
                SAMLIdentityProvider.InitiateSSO(
                    Response,
                    member.GetValue("memberId").ToString(),
                    attribs,
                    targetUrl,
                    partnerSP);
            }


            // Attributes for Morneau Shapell
            if (partnerSP == "SBCSystems")
            {
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
            bool regSuccess;
            string registerApiUrl = "http://" + Request.Url.Host + ":" + Request.Url.Port + "/umbraco/api/HriApi/RegisterUser?userName=" + userName;
            using(var client = new WebClient())
            {
                var result = client.DownloadString(registerApiUrl);
                regSuccess = Convert.ToBoolean(result);
            }

            if (regSuccess)
            {
                // Set the user to be not approved
                MembershipUser memb = Membership.GetUser(userName);
                memb.IsApproved = true;                      
                System.Web.Security.Roles.AddUserToRole(userName, "Registered");
                Membership.UpdateUser(memb); 
                return Redirect("/for-members/login");
            }
            return Content("There was an error validating your account. Your account may have already been validated. Please try logging in at <a href='/' >the site</a> or contact Health Republic New York.");            
        }

        /// <summary>
        /// HttpPost to change the user's email address
        /// </summary>
        /// <param name="model">Change Email model containing password and email address</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            try
            {
                // Get the current user
                var user = Membership.GetUser();
                // Verify the password is correct
                if (model.Email == model.Email2)
                {                  
                    if (Membership.ValidateUser( User.Identity.Name, model.Password))
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
                        // Pass the view model back to the page to persist the data
                        TempData["ViewModel"] = model;
                        // Mark the post as unsuccessful
                        TempData["IsSuccessful"] = false;
                    }                    
                }
                else // Email and confirmation email didnt match
                {
                    // Pass the view model back to the page to persist the data
                    TempData["ViewModel"] = model;
                    // Mark the post as unsuccessful
                    TempData["IsSuccessful"] = false;
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
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var user = Membership.GetUser();
            user.ChangePassword(model.OldPassword, model.NewPassword);
            // Update the User profile in the database
            Membership.UpdateUser((System.Web.Security.MembershipUser)user);
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