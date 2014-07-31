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
        public ActionResult IsUserNameAvailable(string username)
        {
            return Content("false");
        }

        [HttpPost]
        [AllowAnonymous]
        public  ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }


            MembershipCreateStatus status;
            var member = Members.RegisterMember(model, out status, model.LoginOnSuccess);

            switch (status)
            {
                case MembershipCreateStatus.Success:
                    
                    // CUSTOM CODE - Register the user with HRI API
                    string registrationApiString = "";
                    registrationApiString += CurrentPage.GetProperty("ApiRegistrationUrl").Value;
                    
                    WebRequest request = WebRequest.Create(registrationApiString);
                    request.Method = "POST";                    
                    request.Credentials = CredentialCache.DefaultCredentials;
                    //request.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    response.Close();
                    //if there is a specified path to redirect to then use it
                    if (model.RedirectUrl != null || model.RedirectUrl != "")
                    {
                        return Redirect(model.RedirectUrl);
                    }

                    //redirect to current page by default
                    TempData["FormSuccess"] = true;
                    return RedirectToCurrentUmbracoPage();
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


            return CurrentUmbracoPage();
        }

    }
}