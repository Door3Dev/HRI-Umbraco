using Umbraco.Core.Models;
using Umbraco.Web;

namespace HRI.Controllers
{
    /// <summary>
    /// Helper class
    /// </summary>
    public static class Utils
    {
        private static readonly UmbracoHelper Helper = new UmbracoHelper(UmbracoContext.Current);

        public static bool HasAccess(this IPublishedContent content)
        {
            return Helper.MemberHasAccess(content.Id, content.Path);
        }

        public static bool HasAccess(this IContent content)
        {
            return Helper.MemberHasAccess(content.Id, content.Path);
        }
    }
}