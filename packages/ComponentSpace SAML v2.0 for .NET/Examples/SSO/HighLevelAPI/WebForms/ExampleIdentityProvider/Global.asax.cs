using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using ComponentSpace.SAML2.Configuration;
using ComponentSpace.SAML2.Data;

namespace ExampleIdentityProvider {
    public class Global : System.Web.HttpApplication {
        // This method demonstrates loading configuration programmatically.
        // This is useful if you wish to store configuration in a custom database, for example.
        // Alternatively, configuration is loaded automatically from the saml.config file in the application's directory.
        private static void LoadSAMLConfigurationProgrammatically() {
            SAMLConfiguration samlConfiguration = new SAMLConfiguration();

            samlConfiguration.IdentityProviderConfiguration =
                new IdentityProviderConfiguration() {
                    Name = "urn:componentspace:ExampleIdentityProvider",
                    CertificateFile = "idp.pfx",
                    CertificatePassword = "password"
                };

            samlConfiguration.AddPartnerServiceProvider(
                new PartnerServiceProviderConfiguration() {
                    Name = "urn:componentspace:ExampleServiceProvider",
                    WantAuthnRequestSigned = false,
                    SignSAMLResponse = true,
                    SignAssertion = false,
                    EncryptAssertion = false,
                    AssertionConsumerServiceUrl = "http://localhost/ExampleServiceProvider/SAML/AssertionConsumerService.aspx",
                    SingleLogoutServiceUrl = "http://localhost/ExampleServiceProvider/SAML/SLOService.aspx",
                    CertificateFile = "sp.cer"
                });

            SAMLConfiguration.Current = samlConfiguration;
        }

        protected void Application_Start(object sender, EventArgs e) {
        }

        protected void Application_End(object sender, EventArgs e) {
        }
    }
}