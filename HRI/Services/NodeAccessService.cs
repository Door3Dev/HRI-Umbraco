using log4net;
using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace HRI.Services
{
    /// <summary>
    /// Generetas access.config from App_Data\umbracoAccess.config using node path
    /// Allowed to avoid issues when each environment has different page Ids
    /// </summary>
    public class NodeAccessService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(NodeAccessService));

        public void GenerateConfigFile()
        {
            var accessXmlFile = HttpContext.Current.Server.MapPath("~/App_Data/umbracoAccess.config");
            if (System.IO.File.Exists(accessXmlFile))
            {
                // Clear the old access.config.
                System.IO.File.Delete(HttpContext.Current.Server.MapPath("~/App_Data/access.config"));

                var protectedPages = XDocument.Load(accessXmlFile).XPathSelectElements("/access/page");
                foreach (var protectedPage in protectedPages)
                {
                    try
                    {
                        var page = GetPageByUrl(protectedPage.Attribute("path").Value);
                        var loginPage = GetPageByUrl(protectedPage.Attribute("loginPath").Value);
                        var errorPage = GetPageByUrl(protectedPage.Attribute("errorPath").Value);
                        var groups = protectedPage.Attribute("groups").Value.Split(';');

                        Access.ProtectPage(false, page.Id, loginPage.Id, errorPage.Id);
                        foreach (var group in groups)
                        {
                            Access.AddMembershipRoleToDocument(page.Id, group);
                        }
                    }
                    catch(Exception ex)
                    {
                        var message = string.Format("There was an error during access.config generation. Path: '{0}' LoginPath: '{1}' ErrorPath: '{2}'",
                            protectedPage.Attribute("path").Value,
                            protectedPage.Attribute("loginPath").Value,
                            protectedPage.Attribute("errorPath").Value);
                        logger.Error(message, ex);
                    }
                }
            }
        }

        private static IContent GetPageByUrl(string url)
        {
            var pages = url.ToLower().Split('/');
            if (pages.Any())
            {
                var parent = ApplicationContext.Current.Services.ContentService
                    .GetRootContent()
                    .First(x => x.Name.ToLower() == pages.First());
                foreach (var page in pages.Skip(1))
                {
                    parent = parent.Children().First(x => x.Name.ToLower() == page);
                }

                return parent;
            }

            return null;
        }
    }
}