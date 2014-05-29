using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI.Controllers;

namespace Umbraco.Web.UI
{
    public class Global
    {
        public class MyApplication : UmbracoApplication
        {
            // Uncomment to hijack Umbraco's routing
            //protected override void OnApplicationStarting(object sender, EventArgs e)
            //{
            //    DefaultRenderMvcControllerResolver.Current.SetDefaultControllerType(typeof(HomeController));
            //    base.OnApplicationStarting(sender, e);
            //}
        }
    }
}