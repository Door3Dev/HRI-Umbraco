using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;

namespace HRI
{
    public class MyStartupHandler : IApplicationEventHandler
    {
        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {

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
                "TransferRegistration",
                new
                {
                    controller = "HriRegisterController",
                    action = "HandleRegisterMember",
                    id = "0"
                });
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