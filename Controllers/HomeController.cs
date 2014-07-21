using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace HRI.Controllers
{
    public class HomeController : RenderMvcController
    {
        public static class AppSettings
        {
            public const string Attribute = "Attribute";
            public const string PartnerSP = "PartnerSP";
            public const string SubjectName = "SubjectName";
            public const string TargetUrl = "TargetUrl";
        }

        [AllowAnonymous]
        public override ActionResult Index(RenderModel model)
        {
            return base.Index(model);
        }
        
    }
}