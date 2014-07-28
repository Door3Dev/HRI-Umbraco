using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;
using ComponentSpace.SAML2.Bindings;
using ComponentSpace.SAML2.Profiles.ArtifactResolution;
using ComponentSpace.SAML2.Profiles.SSOBrowser;

namespace SAML2ServiceProvider.SAML {
    public partial class AssertionConsumerService : System.Web.UI.Page {
        // The query string parameter identifying the IdP to SP binding in use.
        private const string bindingQueryParameter = "binding";

        // The query string parameter indicating an error occurred.
        private const string errorQueryParameter = "error";

        // Create an absolute URL from an application relative URL.
        private string CreateAbsoluteURL(string relativeURL) {
            return new Uri(Request.Url, ResolveUrl(relativeURL)).ToString();
        }

        // Receive the SAML response from the identity provider.
        private void ReceiveSAMLResponse(ref SAMLResponse samlResponse, ref string relayState) {
            Trace.Write("SP", "Receiving SAML response");

            // Determine the identity provider to service provider binding type.
            // We use a query string parameter rather than having separate endpoints per binding.
            string bindingType = Request.QueryString[bindingQueryParameter];

            // Receive the SAML response over the specified binding.
            XmlElement samlResponseXml = null;

            switch (bindingType) {
                case SAMLIdentifiers.BindingURIs.HTTPPost:
                    ServiceProvider.ReceiveSAMLResponseByHTTPPost(Request, out samlResponseXml, out relayState);
                    break;

                case SAMLIdentifiers.BindingURIs.HTTPArtifact:
                    // Receive the artifact.
                    HTTPArtifact httpArtifact = null;

                    ServiceProvider.ReceiveArtifactByHTTPArtifact(Request, false, out httpArtifact, out relayState);

                    // Create an artifact resolve request.
                    ArtifactResolve artifactResolve = new ArtifactResolve();
                    artifactResolve.Issuer = new Issuer(CreateAbsoluteURL("~/"));
                    artifactResolve.Artifact = new Artifact(httpArtifact.ToString());

                    XmlElement artifactResolveXml = artifactResolve.ToXml();

                    // Send the artifact resolve request and receive the artifact response.
                    string spArtifactResponderURL = WebConfigurationManager.AppSettings["idpArtifactResponderURL"];

                    XmlElement artifactResponseXml = ArtifactResolver.SendRequestReceiveResponse(spArtifactResponderURL, artifactResolveXml);

                    ArtifactResponse artifactResponse = new ArtifactResponse(artifactResponseXml);

                    // Extract the SAML response from the artifact response.
                    samlResponseXml = artifactResponse.SAMLMessage;
                    break;

                default:
                    Trace.Write("SP", "Invalid identity provider to service provider binding");
                    return;
            }

            // Verify the response's signature.
            X509Certificate2 x509Certificate = (X509Certificate2) Application[Global.IdPX509Certificate];

            if (!SAMLMessageSignature.Verify(samlResponseXml, x509Certificate)) {
                throw new ArgumentException("The SAML response signature failed to verify.");
            }

            // Deserialize the XML.
            samlResponse = new SAMLResponse(samlResponseXml);

            Trace.Write("SP", "Received SAML response");
        }

        // Process a successful SAML response.
        private void ProcessSuccessSAMLResponse(SAMLResponse samlResponse, string relayState) {
            Trace.Write("SP", "Processing successful SAML response");

            // Extract the asserted identity from the SAML response.
            SAMLAssertion samlAssertion = (SAMLAssertion) samlResponse.Assertions[0];

            // Get the subject name identifier.
            string userName = samlAssertion.Subject.NameID.NameIdentifier;

            // Get the originally requested resource URL from the relay state.
            RelayState cachedRelayState = RelayStateCache.Remove(relayState);

            if (cachedRelayState == null) {
                Trace.Write("SP", "Nothing in cache");

                return;
            }

            // Create a login context for the asserted identity.
            FormsAuthentication.SetAuthCookie(userName, false);

            // Redirect to the originally requested resource URL.
            Response.Redirect(cachedRelayState.ResourceURL, false);

            Trace.Write("SP", "Processed successful SAML response");
        }

        // Process an error SAML response.
        private void ProcessErrorSAMLResponse(SAMLResponse samlResponse) {
            Trace.Write("SP", "Processing error SAML response");

            string errorMessage = null;

            if ((samlResponse.Status.StatusMessage != null)) {
                errorMessage = samlResponse.Status.StatusMessage.Message;
            }

            string redirectURL = string.Format("~/LoginChoice.aspx?{0}={1}", errorQueryParameter, HttpUtility.UrlEncode(errorMessage));

            Response.Redirect(redirectURL, false);

            Trace.Write("SP", "Processed error SAML response");
        }

        // Process the SAML response returned by the identity provider in response
        // to the authentication request sent by the service provider.
        private void ProcessSAMLResponse() {
            // Receive the SAML response.
            SAMLResponse samlResponse = null;
            string relayState = null;

            ReceiveSAMLResponse(ref samlResponse, ref relayState);

            if (samlResponse == null)
                return;

            // Check whether the SAML response indicates success or an error and process accordingly.
            if (samlResponse.IsSuccess()) {
                ProcessSuccessSAMLResponse(samlResponse, relayState);
            } else {
                ProcessErrorSAMLResponse(samlResponse);
            }
        }

        protected void Page_Load(object sender, EventArgs e) {
            try {
                Trace.Write("SP", "Assertion consumer service");

                ProcessSAMLResponse();
            }

            catch (Exception exception) {
                Trace.Write("SP", "Error in assertion consumer service", exception);
            }
        }
    }
}
