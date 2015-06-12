using System.Collections.Generic;
using HRI.Services;
using log4net;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Models;

namespace HRI.Controllers
{
    public class LoginSurfaceController : HriSufraceController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginSurfaceController));

        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")] LoginModel model)
        {
            // If the model is NOT valid
            if (!ModelState.IsValid)
            {
                // Return the user to the login page
                return CurrentUmbracoPage();
            }

            try
            {
                return HandleLoginCore(model);
            }
            catch (Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "Could not login user " + model.Username + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                Logger.Error("Exception during login", ex);
                ModelState.AddModelError("loginModel", "Oops, we ran into a problem, please try again or contact Member Services for assistance.");
                return CurrentUmbracoPage();
            }
        }

        private ActionResult HandleLoginCore(LoginModel model)
        {
            try
            {
                var member = Services.MemberService.GetByUsername(model.Username);

                // If the user is unable to login
                if (!Members.Login(model.Username, model.Password))
                {
                    // Check to make sure that the user exists
                    const string invalidUsername = "Invalid username. Need to <a href='/for-members/register'>register</a>?";
                    const string invalidPassword = "Invalid credentials. <a href='/for-members/forgot-password'>Click here</a> if you forgot your password?";

                    if (member != null)
                    {
                        if (!Roles.IsUserInRole(model.Username, "Registered")) // User is not activated yet or in process of security upgrade
                        {
                            ModelState.AddModelError(
                                "loginModel",
                                string.Format("One more step! To ensure your privacy, we need to verify your email before you can log in - please check your email inbox for {0} and follow the directions to validate your account.", member.Email));

                            var userService = new UserService();
                            userService.SendVerificationEmail(model.Username);

                            return CurrentUmbracoPage();
                        }

                        if (member.FailedPasswordAttempts >= 2 && member.FailedPasswordAttempts <= 4)
                        {
                            ModelState.AddModelError(
                                "loginModel",
                                string.Format("Your account will be locked after 5 unsuccessful login attempts, please consider resetting your password using <a href='/for-members/forgot-password'>Forgot Password?</a>"));

                            return CurrentUmbracoPage();
                        }

                        if (member.IsLockedOut)
                        {
                            ModelState.AddModelError(
                                "loginModel",
                                string.Format("Your account has been locked, please use <a href='/for-members/forgot-password'>Forgot Password?</a> and follow the steps provided to update password and then login in order to unlock your account."));

                            return CurrentUmbracoPage();
                        }

                        // If the user does exist then it was a wrong password
                        // Don't add a field level error, just model level
                        ModelState.AddModelError("loginModel", invalidPassword);
                        return CurrentUmbracoPage();
                    }

                    // If the user doesn't exists, check the HRI API to see if this is a returning IWS user
                    var currentUmbracoPage = InitiateSecurityUpgradeForIwsUser("loginModel", model.Username);
                    if (currentUmbracoPage != null)
                    {
                        return currentUmbracoPage;
                    }

                    ModelState.AddModelError("loginModel", invalidUsername);
                    return CurrentUmbracoPage();
                }

                var hriUser = MakeInternalApiCallJson("GetRegisteredUserByUsername",
                    new Dictionary<string, string> { { "userName", model.Username } });

                var market = hriUser["Market"].ToString();
                if (string.Compare(member.GetValue<string>("market"), market, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    member.SetValue("market", market);
                }

                if (string.Compare(member.GetValue<string>("market"), "group", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (Roles.IsUserInRole(model.Username, "Billing"))
                    {
                        Roles.RemoveUserFromRole(model.Username, "Billing");
                    }
                }
                else
                {
                    if (!Roles.IsUserInRole(model.Username, "Billing") && Roles.IsUserInRole(model.Username, "Enrolled"))
                    {
                        Roles.AddUserToRole(model.Username, "Billing");
                    }
                }

                if (string.Compare(hriUser["SubscriberFlag"].ToString(), "Y", StringComparison.OrdinalIgnoreCase) ==
                    0)
                {
                    if (!Roles.IsUserInRole("Subscriber"))
                    {
                        Roles.AddUserToRole(model.Username, "Subscriber");
                    }
                }
                else
                {
                    if (Roles.IsUserInRole(model.Username, "Subscriber"))
                    {
                        Roles.RemoveUserFromRole(model.Username, "Subscriber");
                    }
                }

                // Keep Ms First Name and Last Name always up to date
                member.Properties.First(p => p.Alias == "msFirstName").Value = hriUser["MSFirstName"].ToString();
                member.Properties.First(p => p.Alias == "msLastName").Value = hriUser["MSLastName"].ToString();
                member.Properties.First(p=>p.Alias == "healthplanid").Value = hriUser["PlanId"].ToString();
                member.Properties.First(p=>p.Alias == "healthPlanName").Value = hriUser["PlanName"].ToString();
                member.Properties.First(p => p.Alias == "effectiveYear").Value = hriUser["PlanEffectiveDate"].ToString();

                // User should pass enrollment process
                if (member.GetValue<string>("enrollmentpageafterlogin") == "1")
                {
                    // Each time when user trying to login and he is in the enrollment process
                    // has no Group Id and Birthday
                    // we should check the enrollment status through the API call

                    if (String.IsNullOrEmpty(hriUser.Value<string>("RxGrpId"))
                        || String.IsNullOrEmpty(hriUser.Value<string>("DOB")))
                    {
                        return Redirect("/your-account/enrollment-plan-confirmation/");
                    }
                    // Save Group Id, Birthday, Plan Id, Plan Name, add user as enrolled
                    member.Properties.First(p => p.Alias == "groupId").Value = hriUser["RxGrpId"].ToString();
                    member.Properties.First(p => p.Alias == "birthday").Value = hriUser["DOB"].ToString();
                    Roles.AddUserToRole(model.Username, "Enrolled");

                    member.SetValue("enrollmentpageafterlogin", String.Empty);
                }

                Services.MemberService.Save(member);

                //if there is a specified path to redirect to then use it
                if (!string.IsNullOrEmpty(model.RedirectUrl))
                {
                    return Redirect(model.RedirectUrl);
                }

                //redirect to current page by default
                TempData["LoginSuccess"] = true;

                if (Request.Browser.IsMobileDevice)
                    return Redirect("/member-center/index-mobile");

                return Redirect("/member-center/index");
            }
            catch(Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "Could not log in user " + model.Username + ".";
                // Add the error message to the log4net output
                log4net.GlobalContext.Properties["additionalInfo"] = additionalInfo;
                Logger.Error(ex);
                //redirect to current page by default
                TempData["LoginSuccess"] = false;
                return Redirect("/member-center/index");
            }
        }
    }
}