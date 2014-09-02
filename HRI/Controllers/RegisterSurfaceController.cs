using System.Collections.Generic;
using HRI.Models;
using System;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Models;

namespace HRI.Controllers
{
    public class RegisterSurfaceController : HriSufraceController
    {
        [HttpPost]
        [AllowAnonymous]
        public  ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterFormViewModel model)
        {
            // Save Plan Id for the view
            ViewData["PlanId"] = model.PlanId;
            var error = false;

            // Check the Member Id (Y number)
            if (model.PlanId == null && String.IsNullOrEmpty(model.MemberId)
                || !String.IsNullOrEmpty(model.MemberId) && model.MemberId.Length != 9)
            {
                ModelState.AddModelError("registerModel.MemberId", "The Member ID should equal to 9 characters.");
                error = true;
            }

            if (model.PlanId == null)
            {
                var enrolled = MakeInternalApiCall<bool>("IsEnrolledByMemberId",
                    new Dictionary<string, string> {{"memberId", model.MemberId}});

                if (!enrolled)
                {
                    ModelState.AddModelError("registerModel.MemberId", "This Member Id is not enrolled.");
                    error = true;
                }
            }

            // Check SSN number if it's a new member
            if (model.PlanId != null && String.IsNullOrEmpty(model.Ssn))
            {
                ModelState.AddModelError("registerModel.Ssn", "The SSN field is required.");
                error = true;
            }
            // The Y number / username / email should be unique
            var existedUser = MakeInternalApiCallJson("GetRegisteredUserByMemberId", new Dictionary<string, string> { { "memberId", model.Username } });
            if (existedUser != null
                && existedUser.Value<string>("MemberId") == model.MemberId
                && existedUser.Value<string>("UserName") == model.Username
                && existedUser.Value<string>("EMail") == model.Email)
            {
                ModelState.AddModelError("registerModel", "The user with such Member Id, Username and Email has already been registered.");
                error = true;
            }

            if (ModelState.IsValid == false || error)
                return CurrentUmbracoPage();

            // Create registration model and bind it with view model
            var registerModel = RegisterModel.CreateModel();
            registerModel.Name = model.Username;
            registerModel.UsernameIsEmail = false;
            registerModel.Email = model.Email;
            registerModel.Username = model.Username;
            registerModel.Password = model.Password;
            registerModel.RedirectUrl = "for-members/verify-account/";
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "firstName", Value = model.FirstName });
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "lastName", Value = model.LastName});
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "ssn", Value = model.Ssn});
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "zipCode", Value = model.Zipcode});
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "phoneNumber", Value = model.Phone});
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "yNumber", Value = model.MemberId});
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "healthplanid", Value = model.PlanId});
            registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "enrollmentpageafterlogin", Value = Convert.ToInt32(model.PlanId != null).ToString() });

            MembershipCreateStatus status;
            Members.RegisterMember(registerModel, out status, false);
            
            switch (status)
            {
                case MembershipCreateStatus.Success:
                    // Sign the user out (Umbraco wont stop auto logging in - this is a hack to fix)
                    Session.Clear();
                    FormsAuthentication.SignOut();
                    // Set the user to be not approved
                    var memb = Membership.GetUser(model.Username);
                    memb.IsApproved = false;
                    Membership.UpdateUser(memb);
                    // Send the user a verification link to activate their account     
                    var sendVerificationLinkModel = new SendVerificationLinkModel();
                    sendVerificationLinkModel.UserName = model.Username;
                    sendVerificationLinkModel.RedirectUrl = "/for-members/verify-account/";
                    return RedirectToAction("SendVerificationLink_GET", "EmailSurface", sendVerificationLinkModel);

                case MembershipCreateStatus.InvalidUserName:
                    ModelState.AddModelError("registerModel.Username", "Username is not valid");
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
                    ModelState.AddModelError("registerModel.Username", "A member with this username already exists");
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