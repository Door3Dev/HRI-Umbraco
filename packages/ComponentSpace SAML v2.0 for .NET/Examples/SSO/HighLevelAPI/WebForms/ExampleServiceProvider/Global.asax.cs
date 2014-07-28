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

namespace ExampleServiceProvider {
    public class Global : System.Web.HttpApplication {
        // This method demonstrates loading configuration programmatically.
        // This is useful if you wish to store configuration in a custom database, for example.
        // Alternatively, configuration is loaded automatically from the saml.config file in the application's directory.
        private static void LoadSAMLConfigurationProgrammatically() {
            SAMLConfiguration samlConfiguration = new SAMLConfiguration();

            samlConfiguration.ServiceProviderConfiguration = new ServiceProviderConfiguration() {
                Name = "urn:componentspace:ExampleServiceProvider",
                AssertionConsumerServiceUrl = "~/SAML/AssertionConsumerService.aspx",
                CertificateFile = "sp.pfx",
                CertificatePassword = "password"
            };

            samlConfiguration.AddPartnerIdentityProvider(
                new PartnerIdentityProviderConfiguration() {
                    Name = "urn:componentspace:ExampleIdentityProvider",
                    SignAuthnRequest = false,
                    WantSAMLResponseSigned = true,
                    WantAssertionSigned = false,
                    WantAssertionEncrypted = false,
                    SingleSignOnServiceUrl = "http://localhost/ExampleIdentityProvider/SAML/SSOService.aspx",
                    SingleLogoutServiceUrl = "http://localhost/ExampleIdentityProvider/SAML/SLOService.aspx",
                    CertificateFile = "idp.cer"
                });

            SAMLConfiguration.Current = samlConfiguration;
        }

        protected void Application_Start(object sender, EventArgs e) {
        }

        protected void Application_End(object sender, EventArgs e) {
        }
    }
}