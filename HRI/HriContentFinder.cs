using System;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace HRI
{
    public class HriContentFinder : ContentFinderByNiceUrl
    {
        public override bool TryFindContent(PublishedContentRequest contentRequest)
        {
            // User that didn't passed the enrollment process 
            // should see only Enrollment page instead of any member page
            var membershipHelper = new MembershipHelper(UmbracoContext.Current);
            // Next 2 lines from the base class
            var route = !contentRequest.HasDomain ? contentRequest.Uri.GetAbsolutePathDecoded() : contentRequest.Domain.RootNodeId + DomainHelper.PathRelativeToDomain(contentRequest.DomainUri, contentRequest.Uri.GetAbsolutePathDecoded());
            var contentNode = FindContent(contentRequest, route);
            // Page access level/ 10 is member only
            var access = Convert.ToInt32(contentNode.GetProperty("accessAvailablitiy").Value);

            if (membershipHelper.IsLoggedIn() 
                && !UmbracoContext.Current.HttpContext.User.IsInRole("Enrolled")
                && !contentRequest.Uri.GetAbsolutePathDecoded().Contains("/your-account/enrollment")
                && access == 10)
            {
                contentRequest.SetRedirect("/your-account/enrollment/");
                return true;

            }
            return contentNode != null;
        }
    }
}