using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace HRI.Services
{
    public class MemberCenterService
    {
        private readonly UmbracoHelper _helper = new UmbracoHelper(UmbracoContext.Current);
        private readonly string[] _currentMemberRoles;

        public MemberCenterService()
        {
            // Get current member roles
            _currentMemberRoles = Roles.GetRolesForUser();
        }

        public IEnumerable<MemberCenterMenuItem> GetRightMenu()
        {
            // Get the list of right menu items
            var rootNode =
                ((DynamicPublishedContent) _helper.ContentAtRoot().First())
                .DescendantsOrSelf("MemberCenterRightMenuCollection").First();
            var flatList =
                ((DynamicPublishedContent) _helper.ContentAtRoot().First())
                    .Descendants("MemberCenterRightMenuItem")
                    .Where(UserHasAccess)
                    .Select(_ => new MemberCenterMenuItem()
                    {
                        Id = _.Id,
                        ParentId = _.Parent.Id,
                        Title = _.Name,
                        Url = _helper.NiceUrl(_.GetPropertyValue<int>("pageLink")),
                        Description = _.GetPropertyValue<string>("description")
                    });
            // Show items that have subitems or description
            var result = BuildMenuTree(rootNode.Id, flatList)
                            .Where(_ => _.Items.Any() || !String.IsNullOrEmpty(_.Description));
            return result;
        }

        /// <summary>
        /// Checks user rights for the page
        /// </summary>
        /// <param name="content">Content item</param>
        /// <returns>Does the user has an access</returns>
        private bool UserHasAccess(IPublishedContent content)
        {
            // Check is page protected
            var isProtected = Access.IsProtected(content.Id, content.Path);
            // Get page access roles
            var roles = isProtected ? Access.GetAccessingMembershipRoles(content.Id, content.Path) : null;
            // Detect if the user has an access to the page
            var hasAccess = !isProtected || roles.ContainsAny(_currentMemberRoles);
            
            return hasAccess;
        }

        private IEnumerable<MemberCenterMenuItem> BuildMenuTree(int parentId, IEnumerable<MemberCenterMenuItem> originalList)
        {
            var list = new List<MemberCenterMenuItem>();
            originalList.Where(x => x.ParentId == parentId).ForEach(list.Add);
            list.ForEach(x => x.Items = BuildMenuTree(x.Id, originalList));
            return list;
        }
    }


    public class MemberCenterMenuItem
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public IEnumerable<MemberCenterMenuItem> Items;
    }
}