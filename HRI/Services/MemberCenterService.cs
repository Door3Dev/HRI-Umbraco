using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using HRI.Models;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.Security;

namespace HRI.Services
{
    public class MemberCenterService
    {
        private readonly UmbracoHelper _umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
        private readonly MembershipHelper _membershipHelper = new MembershipHelper(UmbracoContext.Current);
        private readonly string[] _currentMemberRoles;

        public MemberCenterService()
        {
            // Get current member roles
            _currentMemberRoles = Roles.GetRolesForUser();
        }

        /// <summary>
        /// Returns Welcome message
        /// </summary>
        /// <returns>Welcome message</returns>
        public string GetWelcomeMessage()
        {
            var member = _membershipHelper.GetCurrentMember();
            var firstName = member.GetPropertyValue<string>("msFirstName");
            var lastName = member.GetPropertyValue<string>("msLastName");
            string welcomeMessage = "Welcome!";
            if (!firstName.IsNullOrWhiteSpace() && !lastName.IsNullOrWhiteSpace())
            {
                welcomeMessage = string.Format("Welcome, {0} {1}!", firstName, lastName);
            }
            else if (!firstName.IsNullOrWhiteSpace() && lastName.IsNullOrWhiteSpace())
            {
                welcomeMessage = string.Format("Welcome, {0}!", firstName);
            }
            return welcomeMessage;
        }

        /// <summary>
        /// Returns user right menu
        /// </summary>
        /// <returns>Hierarchical menu list</returns>
        public IEnumerable<MemberCenterMenuItem> GetRightMenu()
        {
            // Get menu collection root node
            var rootNode =
                ((DynamicPublishedContent) _umbracoHelper.ContentAtRoot().First())
                .DescendantsOrSelf("MemberCenterRightMenuCollection").First();
            // Get the list of all menu items that have proper user rights
            // List has flat structure, next step is to build hierarchy
            var flatList =
                ((DynamicPublishedContent) _umbracoHelper.ContentAtRoot().First())
                    .Descendants("MemberCenterRightMenuItem")
                    .Where(UserHasAccess)
                    .Select(_ => new MemberCenterMenuItem()
                    {
                        Id = _.Id,
                        ParentId = _.Parent.Id,
                        Title = _.Name,
                        Url = _umbracoHelper.NiceUrl(_.GetPropertyValue<int>("pageLink")),
                        Description = _.GetPropertyValue<string>("description")
                    });
            // Build hierarchical menu
            // Show items that have subitems or description
            // Empty items will be skiped
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
        
        /// <summary>
        /// Recursive menu builder
        /// </summary>
        /// <param name="parentId">Parent node ID</param>
        /// <param name="originalList">Original flat menu items list</param>
        /// <returns>Hierarchical menu list</returns>
        private IEnumerable<MemberCenterMenuItem> BuildMenuTree(int parentId, IEnumerable<MemberCenterMenuItem> originalList)
        {
            var list = new List<MemberCenterMenuItem>();
            originalList.Where(x => x.ParentId == parentId).ForEach(list.Add);
            list.ForEach(x => x.Items = BuildMenuTree(x.Id, originalList));
            return list;
        }
    }
}