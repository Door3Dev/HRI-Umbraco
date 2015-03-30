using System.Collections.Generic;
using System.Linq;
using HRI.Controllers;
using HRI.Models;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace HRI.Services
{
    public class MenuService
    {
        /// <summary>
        /// Umbraco node(page) access radio button list values.
        /// The values are come from the database.
        /// </summary>
        private enum NodeAccess { Public = 9, Members = 10, PublicAndMembers = 11 }

        /// <summary>
        /// Umbraco node(page) visibility radio button list.
        /// The values are come from the database.
        /// </summary>
        private enum NodeVisibility { Header = 12, Footer = 13,  HeaderAndFooter = 14, NotVisible = 15 }

        private readonly UmbracoHelper _helper = new UmbracoHelper(UmbracoContext.Current);
        private MenuType _menuType;

        /// <summary>
        /// List menu items
        /// </summary>
        /// <param name="menuType">Menu type items, footer or header for example.</param>
        /// <returns>List of menu items</returns>
        public IEnumerable<MenuItem> ListItems(MenuType menuType)
        {
            // Save the menu type
            _menuType = menuType;
            // Take the home page
            var homePage = ((DynamicPublishedContentList)_helper.ContentAtRoot()).First();
            // Search for the pages that current user has an access
            var nodesList = homePage.Children.Where(UserHasAccess);
            var menu = new List<MenuItem>();

            foreach (var node in nodesList)
            {
                // 1st level items creation
                var menuItem = new MenuItem
                {
                    Label = node.GetPropertyValue<string>("menuItemName"),
                    ShouldOpenInTheNewTab = node.GetPropertyValue<bool>("openInTheNewTab"),
                    Url = node.Url
                };

                // 2nd level items creation
                var subItems = node.Children.Where(UserHasAccess);
                if (subItems.Any())
				{
					foreach (var subnode in node.Children.Where(UserHasAccess))
					{
                        var subMenuItem = new MenuItem
                        {
                            Label = subnode.GetPropertyValue<string>("menuItemName"),
                            ShouldOpenInTheNewTab = subnode.GetPropertyValue<bool>("openInTheNewTab"),
                            Url = subnode.Url,
                            LinkType = subnode.GetPropertyValue<string>("NodeTypeAlias"),
                            FileUrl = subnode.GetPropertyValue<string>("file")
                        };
                        menuItem.Items.Add(subMenuItem);
					}
				}
                menu.Add(menuItem);
			}
            return menu;
        }

        /// <summary>
        /// Checks user rights for the node(page)
        /// </summary>
        /// <param name="content">Content item</param>
        /// <returns>Does the user has an access</returns>
        private bool UserHasAccess(IPublishedContent content)
        {
            // Read the Navigation Visibility node(page) property
            var visibility = content.GetPropertyValue<int>("navigationVisibility");
            // Read the Access Availability node(page) property
            var access = content.GetPropertyValue<int>("accessAvailability");
            // Choose what is restricted for the current user (member or anonymous)
            // Nodes(pages) that are accessible for all will be always included
            var resctrictedAccess = _helper.MemberIsLoggedOn() ? (int)NodeAccess.Public : (int)NodeAccess.Members;
            // Choose allowed node(page) visibility requered during listing
            var allowedVisibility = _menuType == MenuType.Header ? NodeVisibility.Header : NodeVisibility.Footer;

            // Page should have a proper user access (public, members only, etc.)
            // Page should have all Umbraco's rights (role based access)
            // Page should have a proper visibility (header or footer menu)
            return access != resctrictedAccess
                    && content.HasAccess()
                    && (visibility == (int) allowedVisibility || visibility == (int)NodeVisibility.HeaderAndFooter);
        }
    }
}