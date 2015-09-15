using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using HRI.Services;
using HRI.ViewModels;
using log4net;

namespace HRI.Controllers
{
    public class LoginSurfaceController : HriSufraceController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginSurfaceController));

        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")] LoginFormViewModel model)
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
                GlobalContext.Properties["additionalInfo"] = additionalInfo;
                Logger.Error("Exception during login", ex);
                ModelState.AddModelError("loginModel", "Oops, we ran into a problem, please try again or contact Member Services for assistance.");
                return CurrentUmbracoPage();
            }
        }

        private ActionResult HandleLoginCore(LoginFormViewModel model)
        {
            try
            {
                var userService = new UserService();
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

                // Split users on Subscribers and Dependents
                if (string.Compare(hriUser["SubscriberFlag"].ToString(), "Y", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    userService.AddToRole(member.Username, "Subscriber");
                    userService.RemoveFromRole(member.Username, "Dependent");
                    // Group Subscriber should NOT see Billing options
                    if (string.Compare(member.GetValue<string>("market"), "group", StringComparison.OrdinalIgnoreCase) == 0)
                        userService.RemoveFromRole(member.Username, "Billing");
                    else
                        userService.AddToRole(member.Username, "Billing");
                }
                else
                {
                    userService.AddToRole(member.Username, "Dependent");
                    userService.RemoveFromRole(member.Username, "Subscriber");
                    userService.RemoveFromRole(member.Username, "Billing");
                }

                // Keep Ms First Name and Last Name always up to date
                member.Properties.First(p => p.Alias == "msFirstName").Value = hriUser["MSFirstName"].ToString();
                member.Properties.First(p => p.Alias == "msLastName").Value = hriUser["MSLastName"].ToString();
                member.Properties.First(p=>p.Alias == "healthplanid").Value = hriUser["PlanId"].ToString();
                member.Properties.First(p=>p.Alias == "healthPlanName").Value = hriUser["PlanName"].ToString();
                member.Properties.First(p => p.Alias == "effectiveYear").Value = hriUser["PlanEffectiveDate"].ToString();
                member.Properties.First(p => p.Alias == "groupId").Value = hriUser["RxGrpId"].ToString();


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
                    // Save Birthday, add user as enrolled
                    member.Properties.First(p => p.Alias == "birthday").Value = hriUser["DOB"].ToString();
                    userService.AddToRole(model.Username, "Enrolled");

                    member.SetValue("enrollmentpageafterlogin", String.Empty);
                }

                Services.MemberService.Save(member);

                //if there is a specified path to redirect to then use it
                if (!string.IsNullOrEmpty(model.RedirectUrl))
                    return Redirect(model.RedirectUrl);

                // if user asked to save a username
                if (model.RememberMe)
                    userService.RememberUsername(member.Username);
                
                // 180 day Password Expiration Policy
                if ((DateTime.Now - member.LastPasswordChangeDate).TotalDays >= 180)
                {
                    Session["ForcePasswordChange"] = true;
                    return Redirect("/your-account/change-password/");
                }

                //redirect to current page by default
                TempData["LoginSuccess"] = true;

                // Redirect mobile requests to specific page
                if (Request.Browser.IsMobileDevice)
                    return Redirect("/member-center/index-mobile");

                return Redirect("/member-center/index");
            }
            catch(Exception ex)
            {
                // Create an error message with sufficient info to contact the user
                string additionalInfo = "Could not log in user " + model.Username + ".";
                // Add the error message to the log4net output
                GlobalContext.Properties["additionalInfo"] = additionalInfo;
                Logger.Error(ex);
                //redirect to current page by default
                TempData["LoginSuccess"] = false;
                return Redirect("/member-center/index");
            }
        }
    }
}