using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI.Models;

namespace Umbraco.Web.UI.Controllers
{
    public class AboutSurfaceController : SurfaceController
    {
        [HttpGet]
        public ActionResult WhyWeAreDifferent()
        {
            return PartialView("WhyWeAreDifferent");
        }

        [HttpGet]
        public ActionResult ContactUs()
        {
            return View("ContactUs");
        }

        [HttpGet]
        public ActionResult OurStaff()
        {
            return View("OurStaff");
        }

        [HttpGet]
        public ActionResult OurBoard()
        {
            return View("OurBoard");
        }

        [HttpGet]
        public ActionResult Careers()
        {
            return View("Careers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleContactForm(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            MailMessage email = new MailMessage("matthew.wood@door3.com", "matthew.wood@door3.com");
            email.Subject = "HRI - Contact Us";
            email.Body = model.Message + "\n\n" + model.Email;

            try
            {
                SmtpClient smtp = new SmtpClient();
                smtp.Send(email);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            TempData["IsSuccessful"] = true;
            return RedirectToCurrentUmbracoPage();

        }
    }
}