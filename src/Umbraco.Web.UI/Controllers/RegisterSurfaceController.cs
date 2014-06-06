using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI.Models;

namespace Umbraco.Web.UI.Controllers
{
    public class RegisterSurfaceController : SurfaceController
    {
        public ActionResult Index()
        {
            return RedirectToUmbracoPage(1056);
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {               
                return RedirectToUmbracoPage(model.RegistrationSuccessPage);                  
            }
            return CurrentUmbracoPage();
        }
    }
}