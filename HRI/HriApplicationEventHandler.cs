using Umbraco.Core;
using Umbraco.Web.Routing;

namespace HRI
{
    public class HriApplicationEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Insert my finder before ContentFinderByNiceUrl
            ContentFinderResolver.Current.InsertTypeBefore<ContentFinderByNiceUrl, HriContentFinder>();
            // Remove default ContentFinderByNiceUrl
            ContentFinderResolver.Current.RemoveType<ContentFinderByNiceUrl>();

            base.ApplicationStarting(umbracoApplication, applicationContext);
        }
    }
}