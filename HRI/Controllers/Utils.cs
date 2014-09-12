using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace HRI.Controllers
{
    public static class Utils
    {
        public static IEnumerable<IPublishedContent> WhereHasAccess(this IEnumerable<IPublishedContent> content)
        {
            var helper = new UmbracoHelper(UmbracoContext.Current);
            return content.Where(c => !helper.IsProtected(c.Id, c.Path) || helper.MemberHasAccess(c.Id, c.Path));
        }

        public static bool HasAccess(this IPublishedContent content)
        {
            var helper = new UmbracoHelper(UmbracoContext.Current);
            return !helper.IsProtected(content.Id, content.Path) || helper.MemberHasAccess(content.Id, content.Path);
        }
    }
}