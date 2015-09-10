using System.Web.Optimization;
using HRI.Services;
using Umbraco.Core;
using Umbraco.Web.Routing;

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

            // Generate access.config
            var accessService = new NodeAccessService();
            accessService.GenerateConfigFile();

            // Prepare DB with new updates
            var deployService = new DeployService();
            deployService.Install();
        }

        private void RegisterStyles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/css")
                .Include("~/css/bootstrap.css")
                .Include("~/css/fonts.css")
                .Include("~/css/styles.css"));

            bundles.Add(new StyleBundle("~/bundles/css/mobile")
                .Include("~/css/mobile.css"));
        }

        private void RegisterJavaScript(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js")
                .Include("~/Scripts/jquery-1.9.1.js")
                .Include("~/Scripts/jquery.inputmask/jquery.inputmask.js")
                .Include("~/scripts/jquery.inputmask/jquery.inputmask.regex.extensions.js")
                .Include("~/scripts/jquery.validate.min.js")
                .Include("~/scripts/jquery.validate.unobtrusive.min.js"));
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
            ContentFinderResolver.Current.InsertTypeBefore<ContentFinderByNiceUrl, HriContentFinder>();
            // Remove standart ContentFinderByNiceUrl
            ContentFinderResolver.Current.RemoveType<ContentFinderByNiceUrl>();
        }
    }
}