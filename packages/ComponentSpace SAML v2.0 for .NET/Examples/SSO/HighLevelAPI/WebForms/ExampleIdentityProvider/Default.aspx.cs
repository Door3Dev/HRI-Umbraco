using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using ComponentSpace.SAML2;

namespace ExampleIdentityProvider {
    public static class AppSettings {
        public const string Attribute = "Attribute";
        public const string PartnerSP = "PartnerSP";
        public const string SubjectName = "SubjectName";
        public const string TargetUrl = "TargetUrl";
    }

    public partial class _Default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
        }

        protected void ssoLinkButton_Click(object sender, EventArgs e) {
            // Initiate single sign-on to the service provider (IdP-initiated SSO)
            // by sending a SAML response containing a SAML assertion to the SP.
            // Use the configured or logged in user name as the user name to send to the service provider (SP).
            // Include some user attributes.
            // If no target URL is specified the SP should display its default page.
            string partnerSP = WebConfigurationManager.AppSettings[AppSettings.PartnerSP];
            string targetUrl = WebConfigurationManager.AppSettings[AppSettings.TargetUrl];

            string userName = WebConfigurationManager.AppSettings[AppSettings.SubjectName];

            if (string.IsNullOrEmpty(userName)) {
                userName = User.Identity.Name;
            }

            IDictionary<string, string> attributes = new Dictionary<string, string>();

            foreach (string key in WebConfigurationManager.AppSettings.Keys) {
                if (key.StartsWith(AppSettings.Attribute)) {
                    attributes[key.Substring(AppSettings.Attribute.Length + 1)] = WebConfigurationManager.AppSettings[key];
                }
            }

            SAMLIdentityProvider.InitiateSSO(
                Response,
                userName,
                attributes,
                targetUrl,
                partnerSP);
        }

        protected void logoutButton_Click(object sender, EventArgs e) {
            // Logout locally.
            FormsAuthentication.SignOut();

            if (SAMLIdentityProvider.IsSSO()) {
                // Request logout at the service providers.
                SAMLIdentityProvider.InitiateSLO(Response, null);
            } else {
                FormsAuthentication.RedirectToLoginPage();
            }
        }
    }
}
