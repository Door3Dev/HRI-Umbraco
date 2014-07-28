/*
 * This example demonstrates XML signature generation and verification using SHA-256 digest and signature algorithms.
 * SHA-256 XML signature supports requires either .NET 4.5 or above, or a .NET CLR security update.
 * Refer to the Developer Guide for instructions on enabling .NET 4.5 support or installing and configuring the .NET CLR security update.
 * .NET 3.5 or higher is required for the CLR security update.
 * 
 * For many applications, the default SHA1 algorithms are perfectly suitable.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

#if DOTNET45
// Only supported in .NET 4.5 and above - add a reference to the System.Deployment assembly.
using System.Deployment.Internal.CodeSigning;
#endif

using ComponentSpace.SAML2.Bindings;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;

namespace SHA256Signature {
    class Program {
        static void Main(string[] args) {
            try {
#if DOTNET45
                // Register the SHA-256 cryptographic algorithm.
                // Only supported in .NET 4.5 and above.
                CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
#endif

                // Load the certificate and private key for signature generation.
                X509Certificate2 x509Certificate = new X509Certificate2("idp.pfx", "password");

                // Create a basic SAML assertion and serialize it to XML.
                SAMLAssertion samlAssertion = new SAMLAssertion();
                samlAssertion.Issuer = new Issuer("test");
                XmlElement samlAssertionElement = samlAssertion.ToXml();

                // Sign the SAML assertion using SHA-256 for the digest and signature algorithms.
                SAMLAssertionSignature.Generate(samlAssertionElement, x509Certificate.PrivateKey, x509Certificate, null, "http://www.w3.org/2001/04/xmlenc#sha256", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
                Console.WriteLine("Signed SAML assertion: {0}", samlAssertionElement.OuterXml);

                // Verify the signature.
                bool verified = SAMLAssertionSignature.Verify(samlAssertionElement);
                Console.WriteLine("Signature verified: {0}", verified);

                // The HTTP-redirect doesn't use XML signatures so check it separately.
                // Create a basic authn request and serialize it to XML.
                AuthnRequest authnRequest = new AuthnRequest();
                authnRequest.Issuer = new Issuer("test");
                XmlElement authnRequestElement = authnRequest.ToXml();

                // Create the HTTP-redirect URL included the SHA-256 signature.
                string url = HTTPRedirectBinding.CreateRequestRedirectURL("http://www.test.com", authnRequestElement, null, x509Certificate.PrivateKey, HTTPRedirectBinding.SignatureAlgorithms.RSA_SHA256);

                string relayState = null;
                bool signed = false;

                // Retrieve the authn request from the HTTP-redirect URL and verify the signature.
                HTTPRedirectBinding.GetRequestFromRedirectURL(url, out authnRequestElement, out relayState, out signed, x509Certificate.PublicKey.Key);
            }

            catch (Exception exception) {
                // If signature generation/verification fails then most likely the .NET CLR security update 
                // hasn't been installed and configured correctly or the inbuilt .NET SHA-256 support hasn't been initialized.
                Console.WriteLine(exception.ToString());
            }
        }
    }
}
