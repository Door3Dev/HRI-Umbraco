using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using ComponentSpace.SAML2;

namespace ExampleServiceProvider {
    public static class AppSettings {
        public const string PartnerIdP = "PartnerIdP";
    }

    public partial class Login : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
        }

        protected void ssoLinkButton_Click(object sender, EventArgs e) {
            // To login at the service provider, initiate single sign-on to the identity provider (SP-initiated SSO).
            string partnerIdP = WebConfigurationManager.AppSettings[AppSettings.PartnerIdP];
            SAMLServiceProvider.InitiateSSO(Response, null, partnerIdP);
        }
    }
}
