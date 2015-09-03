using System;
using System.Web.Security;
using Umbraco.Web.Routing;

namespace HRI
{
    public class HriContentFinder : ContentFinderByNiceUrl
    {
        public override bool TryFindContent(PublishedContentRequest contentRequest)
        {
            // 180 day Password Expiration Policy
            var member = Membership.GetUser();
            if (member != null && (DateTime.Now - member.LastPasswordChangedDate).TotalDays >= 180
                && !contentRequest.Uri.AbsoluteUri.Contains("your-account/change-password"))
            {
                contentRequest.SetRedirect("/your-account/change-password/");
                return true;
            }
            return base.TryFindContent(contentRequest);
        }
    }
}