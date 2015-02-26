using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Umbraco.Core;

namespace HRI
{
    public class MyStartupHandler : IApplicationEventHandler
    {
        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Comment this out to control this setting via web.config compilation debug attribute
            BundleTable.EnableOptimizations = true; 

            RegisterJavaScript(BundleTable.Bundles);
            RegisterStyles(BundleTable.Bundles);

            // SAML Controller
            RouteTable.Routes.MapRoute(
                "",
                "SAML",
                new
                {
                    controller = "SAML",
                    action = "Index",
                    id = "0"
                });

            RouteTable.Routes.MapRoute(
                "",
                "SAML/SSO",
                new
                {
                    controller = "News",
                    action = "SSO",
                    id = "1"
                });

            RouteTable.Routes.MapRoute(
                "",
                "SAML/SLO",
                new
                {
                    controller = "News",
                    action = "SLO",
                    id = "2"
                });

            RouteTable.Routes.MapRoute(
                "",
                "RegisterUser",
                new
                {
                    controller = "HriApiController",
                    action = "RegisterUser",
                    id = "0"
                });
        }

        private void RegisterStyles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/css")
                .Include("~/css/bootstrap.css")
                .Include("~/css/fonts.css")
                .Include("~/css/styles.css"));
        }

        private void RegisterJavaScript(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js")
                .Include("~/Scripts/jquery-1.9.1.js")
                .Include("~/Scripts/jquery.inputmask/jquery.inputmask.js")
                .Include("~/scripts/jquery.inputmask/jquery.inputmask.regex.extensions.js"));
        }

        public void OnApplicationInitialized(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(
            UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {
        }
    }
}