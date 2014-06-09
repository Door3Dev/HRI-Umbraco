using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI.Models;

namespace Umbraco.Web.UI.Controllers
{
    public class RegisterSurfaceController : SurfaceController
    {
        public ActionResult Index()
        {            
            return View("Register");
        }

        [HttpPost]
        public ActionResult Register(RegisterFormViewModel model)
        {
            if(ModelState.IsValid)
            {               
                return RedirectToUmbracoPage(model.RegistrationSuccessPage);                  
            }
            return CurrentUmbracoPage();
        }
    }
}