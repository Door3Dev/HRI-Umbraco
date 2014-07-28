using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using ComponentSpace.SAML2;

namespace ExampleServiceProvider.SAML {
    public partial class AssertionConsumerService : System.Web.UI.Page {
        public const string AttributesSessionKey = "";

        protected void Page_Load(object sender, EventArgs e) {
            bool isInResponseTo = false;
            string partnerIdP = null;
            string userName = null;
            IDictionary<string, string> attributes = null;
            string targetUrl = null;

            // Receive and process the SAML assertion contained in the SAML response.
            // The SAML response is received either as part of IdP-initiated or SP-initiated SSO.
            SAMLServiceProvider.ReceiveSSO(Request, out isInResponseTo, out partnerIdP, out userName, out attributes, out targetUrl);

            // If no target URL is provided, provide a default.
            if (targetUrl == null) {
                targetUrl = "~/";
            }

            // Login automatically using the asserted identity.
            // This example uses forms authentication. Your application can use any authentication method you choose.
            // There are no restrictions on the method of authentication.
            FormsAuthentication.SetAuthCookie(userName, false);

            // Save the attributes.
            Session[AttributesSessionKey] = attributes;

            // Redirect to the target URL.
            Response.Redirect(targetUrl, false);
        }
    }
}
