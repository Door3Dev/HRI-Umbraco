using System.Collections.Generic;
using System.Linq;
using HRI.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using EnumerableExtensions = Umbraco.Core.EnumerableExtensions;

namespace HRI.Services
{
    public class SiteMapService
    {
        private readonly UmbracoHelper _helper = new UmbracoHelper(UmbracoContext.Current);

        public IList<SiteMapItem> List(int level)
        {
            var home = ((DynamicPublishedContent) _helper.ContentAtRoot().First());
            var flatList = home.Descendants()
                .Where(_ => _.Visible && _.Level <= level && !_.GetPropertyValue<bool>("hideInSitemap"))
                .Select(x => new SiteMapItem()
                {
                    Id = x.Id,
                    ParentId = x.Parent.Id,
                    Level = x.Level,
                    Name = x.Name,
                    Url = x.Url

                })
                .ToList();
            var result = BuildTree(flatList, home.Id);

            return result;
        }

        /// <summary>
        /// Recursive menu builder
        /// </summary>
        /// <param name="flatList">Original flat menu items list</param>
        /// <param name="parentId">Parent node ID</param>
        /// <returns>Hierarchical list</returns>
        private IList<SiteMapItem> BuildTree(IList<SiteMapItem> flatList, int parentId)
        {
            var list = new List<SiteMapItem>();
            EnumerableExtensions.ForEach(flatList.Where(x => x.ParentId == parentId), list.Add);
            list.ForEach(x => x.Children = BuildTree(flatList, x.Id));
            return list;
        }
    }
}