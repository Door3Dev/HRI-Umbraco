using System.Collections.Generic;
using System.Data.SqlTypes;
using HRI.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Examine;
using Umbraco.Web.Models;

namespace HRI.Controllers
{
    public class RegisterSurfaceController : HriSufraceController
    {
        public const string PasswordNotStrongEnough = "The password is not strong enough";

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")] RegisterFormViewModel model)
        {
            try
            {
                // Save Plan Id for the view
                ViewData["PlanId"] = model.PlanId;

                var enrollAfterLogin = Convert.ToInt32(model.PlanId != null).ToString();

                // Check the Member Id (Y number)
                if (model.PlanId == null) // Enrolled user
                {
                    ModelState.Remove("registerModel.Zipcode");

                    var errorMessage = ValidateMemberIdCore(model.MemberId, model.DateOfBirth, true);

                    if (errorMessage != null)
                    {
                        ModelState.AddModelError("registerModel.MemberId", errorMessage);
                    }

                    // check if yNumber is already registered in umbraco
                    var registeredUsername = GetMemberRegisteredUsernameWithYNumber(model.MemberId);
                    if (registeredUsername != null)
                    {
                        ModelState.AddModelError("registerModel.MemberId", "The Member ID you have entered is registered with existing user name: " + registeredUsername);
                    }

                    // if there's no error, try to get plan ID from api
                    var planId = MakeInternalApiCall<string>("GetHealthPlanIdByMemberId",
                        new Dictionary<string, string> { { "memberId", model.MemberId } });
                    if (planId != null)
                    {
                        ViewData["PlanId"] = model.PlanId;
                        model.PlanId = planId;
                    }
                }
                else
                {
                    // is new user
                    // Validate ZipCode
                    if (!ComparePlansSurfaceController.IsValidZipCodeInternal(model.Zipcode))
                    {
                        ModelState.AddModelError("registerModel.Zipcode", "Invalid Zip Code");
                    }
                }

                if (!ModelState.IsValid)
                    return CurrentUmbracoPage();

                // Create registration model and bind it with view model
                var registerModel = RegisterModel.CreateModel();
                registerModel.Name = model.Username.Trim();
                registerModel.UsernameIsEmail = false;
                registerModel.Email = model.Email.Trim();
                registerModel.Username = model.Username.Trim();
                registerModel.Password = model.Password.Trim();
                registerModel.RedirectUrl = "for-members/verify-account/";
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "firstName", Value = model.FirstName.Trim() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "lastName", Value = model.LastName.Trim() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "ssn", Value = model.Ssn.TrimNullable() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "zipCode", Value = model.Zipcode.TrimNullable() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "phoneNumber", Value = model.Phone.TrimNullable() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "yNumber", Value = model.MemberId.TrimNullable() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "healthplanid", Value = model.PlanId.TrimNullable() });
                registerModel.MemberProperties.Add(new UmbracoProperty { Alias = "enrollmentpageafterlogin", Value = enrollAfterLogin });

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
                        ModelState.AddModelError("registerModel.Password", PasswordNotStrongEnough);
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
            catch (Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "Could not register user " + model.Username + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                return CurrentUmbracoPage();
            }
        }

        public JsonResult ValidateMemberId([Bind(Prefix = "registerModel")] RegisterFormViewModel model)
        {
            var errorMsg = ValidateMemberIdCore(model.MemberId, model.DateOfBirth);

            return errorMsg == null 
                ? Json(true, JsonRequestBehavior.AllowGet) : Json(errorMsg, JsonRequestBehavior.AllowGet);
        }

        private string ValidateMemberIdCore(string memberId, DateTime dateOfBirth, bool forceDOB = false)
        {
            if (memberId == null || memberId.Length != 9)
            {
                return "The Member ID should have 9 characters.";
            }

            var user = MakeInternalApiCallJson(
                "IsUserWithMemberIdRegistered",
                new Dictionary<string, string> {{"memberId", memberId}});

            if (user != null)
            {
                var errorMsg = string.Format(
                    "The Member ID you have entered is registered with existing user name: {0}",
                    user.Value<string>("username"));

                return errorMsg;
            }

            var dob = (dateOfBirth == default(DateTime)) ? null : dateOfBirth.ToString("yyyy-MM-dd");

            if (forceDOB && dob == null) dob = SqlDateTime.MinValue.Value.ToString("yyyy-MM-dd");

            var enrolled = 
            MakeInternalApiCall<bool>("IsEnrolledByMemberId",
                new Dictionary<string, string> {{"memberId", memberId}, {"DOB", dob }});

            return enrolled
                ? null
                : ((dob == null)
                    ? "This Member ID is not enrolled."
                    : "Our records don't match this Member ID with this DOB, please be sure these are accurate. This verification is necessary to protect the identity of our members.");
        }

        private string GetMemberRegisteredUsernameWithYNumber(string yNumber)
        {
            var memberSearcher = ExamineManager.Instance.SearchProviderCollection["InternalMemberSearcher"];

            var criteria = memberSearcher.CreateSearchCriteria("member").Field("yNumber", yNumber).Compile();

            var members = memberSearcher.Search(criteria);

            var registeredMember = members.FirstOrDefault();

            // Fields["nodeName"] = username of the member
            return registeredMember != null ? registeredMember.Fields["nodeName"] : null;
        }
    }
}