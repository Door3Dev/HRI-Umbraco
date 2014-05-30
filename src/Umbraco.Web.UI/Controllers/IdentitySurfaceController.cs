using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI.Models;

namespace Umbraco.Web.UI.Controllers
{
    public class IdentitySurfaceController : SurfaceController
    {
        public ActionResult Index(RenderModel model)
        {
            return View("Login", new LoginViewModel());
        }

        public ActionResult Login()
        {
            return PartialView("Login", new LoginViewModel());
        }
    }
}