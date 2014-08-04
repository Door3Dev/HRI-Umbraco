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

        public ActionResult SingleSignOn(IDictionary<string, string> attributes, string targetUrl, string partnerSP)
        {
            // Initiate single sign-on to the service provider (IdP-initiated SSO)]
            // by sending a SAML response containing a SAML assertion to the SP.
            
            // If this is a door3 user we can grab our user number from the database            
            string memberID = Services.MemberService.GetByUsername(User.Identity.Name).Properties.Where(p => p.Alias == "primaryMemberID").First().Value.ToString();

            Dictionary<string, string> attribs = new Dictionary<string, string>();

            // Attributes for MagnaCare
            attribs.Add("Member ID", Services.MemberService.GetByUsername(User.Identity.Name).Properties.Where(p => p.Alias == "primaryMemberID").First().Value.ToString());
            attribs.Add("First Name", Services.MemberService.GetByUsername(User.Identity.Name).Properties.Where(p => p.Alias == "firstName").First().Value.ToString());
            attribs.Add("Last Name", Services.MemberService.GetByUsername(User.Identity.Name).Properties.Where(p => p.Alias == "lastName").First().Value.ToString());
            attribs.Add("Product", "PRIMARYSELECT");

            // Otherwise we must use the HRI api to determine the user ID for the given username
            // Update the door3 database to have this user number for future uses            

            SAMLIdentityProvider.InitiateSSO(
                Response,
                memberID,
                attribs,
                targetUrl,
                partnerSP);

            ViewBag.Response = Response;
            return new EmptyResult();
        }

        

        public ActionResult ActivateUser(string userName, string guid)
        {
            var m = Services.MemberService.GetByEmail(email);
            m.IsApproved = true;

            string regData="";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://23.253.132.105:64102/api/Registration");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = regData.Length;
            using (Stream webStream = request.GetRequestStream())
            {
                using(StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(regData);
                }
            }

            try
            {
                WebResponse webResponse = request.GetResponse();
                using(Stream webstream = webResponse.GetResponseStream())
                {

                }
            }
            catch (Exception ex)
            { }
            // Register with API here
            
            return Redirect("/");
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