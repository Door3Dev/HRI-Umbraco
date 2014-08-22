using HRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Controllers;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class RegisterSurfaceController : SurfaceController
    {
        

        [HttpPost]
        [AllowAnonymous]
        public  ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterModel model)
        {
            if (ModelState.IsValid == false)
            {
                TempData["IsNotValid"] = true;
                return CurrentUmbracoPage();
            }

            model.Name = model.Username;
            MembershipCreateStatus status;
            var member = Members.RegisterMember(model, out status, false);
            
            switch (status)
            {
                case MembershipCreateStatus.Success:
                    // Sign the user out (Umbraco wont stop auto logging in - this is a hack to fix)
                    Session.Clear();
                    FormsAuthentication.SignOut();
                    // Set the user to be not approved
                    MembershipUser memb = Membership.GetUser(model.Username);
                    memb.IsApproved = false;
                    Membership.UpdateUser(memb);
                    // Send the user a verification link to activate their account     
                    SendVerificationLinkModel sendVerificationLinkModel = new SendVerificationLinkModel();
                    sendVerificationLinkModel.UserName = model.Username;
                    sendVerificationLinkModel.RedirectUrl = "/for-members/verify-account/";
                    return RedirectToAction("SendVerificationLink_GET", "EmailSurface", sendVerificationLinkModel);

                case MembershipCreateStatus.InvalidUserName:
                    ModelState.AddModelError((model.UsernameIsEmail || model.Username == null)
                        ? "registerModel.Email"
                        : "registerModel.Username",
                        "Username is not valid");
                    break;
                case MembershipCreateStatus.InvalidPassword:
                    ModelState.AddModelError("registerModel.Password", "The password is not strong enough");
                    break;
                case MembershipCreateStatus.InvalidQuestion:
                case MembershipCreateStatus.InvalidAnswer:
                    //TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
                    throw new NotImplementedException(status.ToString());
                case MembershipCreateStatus.InvalidEmail:
                    ModelState.AddModelError("registerModel.Email", "Email is invalid");
                    break;
                case MembershipCreateStatus.DuplicateUserName:
                    ModelState.AddModelError((model.UsernameIsEmail || model.Username == null)
                        ? "registerModel.Email"
                        : "registerModel.Username",
                        "A member with this username already exists.");
                    break;
                case MembershipCreateStatus.DuplicateEmail:
                    ModelState.AddModelError("registerModel.Email", "A member with this e-mail address already exists");
                    break;
                case MembershipCreateStatus.UserRejected:
                case MembershipCreateStatus.InvalidProviderUserKey:
                case MembershipCreateStatus.DuplicateProviderUserKey:
                case MembershipCreateStatus.ProviderError:
                    //don't add a field level error, just model level
                    ModelState.AddModelError("registerModel", "An error occurred creating the member: " + status);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (ModelState.Count > 0)
                TempData["IsNotValid"] = true;


            return CurrentUmbracoPage();
        }

    }
}