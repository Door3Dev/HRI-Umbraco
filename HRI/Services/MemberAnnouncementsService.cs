using System.Collections.Generic;
using System.Linq;
using HRI.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace HRI.Services
{
    public class MemberAnnouncementsService
    {
        private readonly UmbracoHelper _helper = new UmbracoHelper(UmbracoContext.Current);

        public IEnumerable<MemberAnnouncement> GetList()
        {
            // Get the list of announcements that are visible
            var result =
                ((DynamicPublishedContent) _helper.ContentAtRoot().First())
                    .DescendantsOrSelf("MemberCenterAnnouncementItem")
                    .Where(_ => _.GetPropertyValue<bool>("visible"))
                    .Select(_ => new MemberAnnouncement()
                    {
                        Title = _.GetPropertyValue<string>("title"),
                        ShortMessage = _.GetPropertyValue<string>("shortMessage"),
                        FullMessage = _.GetPropertyValue<string>("fullMessage"),
                        Url = _.Url
                    });

            return result;
        }
    }
}