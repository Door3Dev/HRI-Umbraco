//-----------------------------------------------------------------------
// <copyright file="SAML20Assertion.cs" company="CoverMyMeds">
//  Copyright (c) 2012 CoverMyMeds.  All rights reserved.
//  This code is presented as reference material only.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.IO;
using ComponentSpace.SAML2.Configuration;
using CoverMyMeds.SAML.Library.Schema;
using NLog;

namespace CoverMyMeds.SAML.Library
{
    /// <summary>
    /// Encapsulate functionality for building a SAML Response using the Schema object
    ///     created by xsd.exe from the OASIS spec
    /// </summary>
    /// <remarks>Lots of guidance from this CodeProject implementation
    ///     http://www.codeproject.com/Articles/56640/Performing-a-SAML-Post-with-C#xx0xx
    /// </remarks>
    public class SAML20Assertion
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Build a signed XML SAML Response string to be inlcuded in an HTML Form
        /// for POSTing to a SAML Service Provider
        /// </summary>
        /// <param name="Issuer">Identity Provider - Used to match the certificate for verifying 
        ///     Response signing</param>
        /// <param name="AssertionExpirationMinutes">Assertion lifetime</param>
        /// <param name="Audience"></param>
        /// <param name="Subject"></param>
        /// <param name="Recipient"></param>
        /// <param name="Attributes">Dictionary of attributes to send through for user SSO</param>
        /// <param name="SigningCert">X509 Certificate used to sign Assertion</param>
        /// <returns></returns>
        public static string CreateSAML20Response(string Issuer,
            int AssertionExpirationMinutes,
            string Audience,
            string Subject,
            string Recipient,
            Dictionary<string, string> Attributes,
            string partnerSP)
        {
            // Create SAML Response object with a unique ID and correct version
            var response = new ResponseType()
            {
                ID = "_" + Guid.NewGuid(),
                Version = "2.0",
                IssueInstant = System.DateTime.UtcNow,
                Destination = Recipient.Trim(),
                Issuer = new NameIDType() { Value = Issuer.Trim() },
                Status = new StatusType() { StatusCode = new StatusCodeType() { Value = "urn:oasis:names:tc:SAML:2.0:status:Success" } }
            };

            // Put SAML 2.0 Assertion in Response
            response.Items = new AssertionType[] { CreateSAML20Assertion(Issuer, AssertionExpirationMinutes, Audience, Subject, Recipient, Attributes) };

            XmlDocument XMLResponse = SerializeAndSignSAMLResponse(response, partnerSP);

            return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(XMLResponse.OuterXml));
            //return XMLResponse.OuterXml;
        }

        public static void GuideSSO(HttpResponseBase httpResponse, string partnerSp, string subject, Dictionary<string, string> samlAttributes)
        {
            SAMLConfiguration.Load();
            var issuer = SAMLConfiguration.Current.IdentityProviderConfiguration.Name;
            var partner = SAMLConfiguration.Current.GetPartnerServiceProvider(partnerSp);

            var saml = CreateSAML20Response(issuer, 5, partnerSp,
                    subject,
                    partner.AssertionConsumerServiceUrl,
                    samlAttributes,
                    partnerSp);

            var responseContent = String.Format("<html xmlns=\"http://www.w3.org/1999/xhtml\">"
            + "<body onload=\"document.forms.samlform.submit()\">"
            + "<noscript><p><strong>Note:</strong> Since your browser does not support Javascript, you must press the Continue button once to proceed.</p></noscript>"
            + "<form id=\"samlform\" action=\"{0}\" method=\"post\">"
            + "<div>"
            + "<input type=\"hidden\" name=\"SAMLResponse\" value=\"{1}\" />"
            + "<input type=\"hidden\" name=\"RelayState\" value=\"\" />"
            + "</div><noscript><div><input type=\"submit\" value=\"Continue\" /></div></noscript>"
            + "</form>"
            + "</body>"
            + "</html>", partner.AssertionConsumerServiceUrl, saml);
            httpResponse.Write(responseContent);

        }

        /// <summary>
        /// Accepts SAML Response, serializes it to XML and signs using the supplied certificate
        /// </summary>
        /// <param name="Response">SAML 2.0 Response</param>
        /// <param name="SigningCert">X509 certificate</param>
        /// <returns>XML Document with computed signature</returns>
        private static XmlDocument SerializeAndSignSAMLResponse(ResponseType Response, string partnerSP)
        {
            // Set serializer and writers for action
            XmlSerializer responseSerializer = new XmlSerializer(Response.GetType());
            StringWriter stringWriter = new StringWriter();
            XmlWriter responseWriter = XmlTextWriter.Create(stringWriter, new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, Encoding = Encoding.UTF8 });
            responseSerializer.Serialize(responseWriter, Response);
            responseWriter.Close();
            XmlDocument xmlResponse = new XmlDocument(); 
            xmlResponse.LoadXml(stringWriter.ToString());
            XmlElement elementToEncrypt = xmlResponse.GetElementsByTagName("saml:Assertion")[0] as XmlElement;
            if (elementToEncrypt == null)
            {
                elementToEncrypt = xmlResponse.GetElementsByTagName("Assertion")[0] as XmlElement;
            }

            //////////////////////////////////////////////////
            // Create a new instance of the EncryptedXml class 
            // and use it to encrypt the XmlElement with the 
            // X.509 Certificate.
            //////////////////////////////////////////////////

            EncryptedXml eXml = new EncryptedXml();

            // Encrypt the element.
            var certificate = SAMLConfiguration.Current.CertificateManager.GetPartnerCertificate(partnerSP);
            EncryptedData edElement = eXml.Encrypt(elementToEncrypt, certificate);

            XmlElement encryptedAssertion = xmlResponse.CreateElement("saml2", "EncryptedAssertion", "urn:oasis:names:tc:SAML:2.0:assertion");
            EncryptedXml.ReplaceElement(encryptedAssertion, edElement, true);
            
            
            ////////////////////////////////////////////////////
            // Replace the element from the original XmlDocument
            // object with the EncryptedData element.
            ////////////////////////////////////////////////////
            xmlResponse.GetElementsByTagName("Response")[0].ReplaceChild(encryptedAssertion, elementToEncrypt);

            //Get the X509 info
            XmlElement issuerSerialElement = xmlResponse.CreateElement("ds", "X509IssuerSerial", SignedXml.XmlDsigNamespaceUrl);
            XmlElement issuerNameElement = xmlResponse.CreateElement("ds", "X509IssuerName", SignedXml.XmlDsigNamespaceUrl);
            issuerNameElement.AppendChild(xmlResponse.CreateTextNode(certificate.IssuerName.Name));
            issuerSerialElement.AppendChild(issuerNameElement);
            XmlElement serialNumberElement = xmlResponse.CreateElement("ds", "X509SerialNumber", SignedXml.XmlDsigNamespaceUrl);
            serialNumberElement.AppendChild(xmlResponse.CreateTextNode(certificate.SerialNumber));
            issuerSerialElement.AppendChild(serialNumberElement);

            // Set requested namespaces
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["EncryptedData"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["EncryptionMethod"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["KeyInfo"].Prefix = "ds";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["EncryptedKey"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["EncryptionMethod"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["CipherData"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["xenc:CipherData"]["CipherValue"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["KeyInfo"].Prefix = "ds";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["ds:KeyInfo"]["X509Data"].Prefix = "ds";
            // We don't need this for MagnaCare
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["ds:KeyInfo"]["ds:X509Data"].RemoveChild(xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["ds:KeyInfo"]["ds:X509Data"]["X509Certificate"]);
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["CipherData"].Prefix = "xenc";
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["xenc:CipherData"]["CipherValue"].Prefix = "xenc";

            //Add X509 Issuer Info 
            xmlResponse.DocumentElement["saml2:EncryptedAssertion"]["xenc:EncryptedData"]["ds:KeyInfo"]["xenc:EncryptedKey"]["ds:KeyInfo"]["ds:X509Data"].AppendChild(issuerSerialElement);

            // Set the namespace for prettire and more consistent XML
            var ns = new XmlNamespaceManager(xmlResponse.NameTable);
            ns.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            ns.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");

            CertificateUtility.AppendSignatureToXMLDocument(ref xmlResponse, "#" + ((AssertionType)Response.Items[0]).ID);

            return xmlResponse;
        }

        /// <summary>
        /// Creates a SAML 2.0 Assertion Segment for a Response
        /// Simple implmenetation assuming a list of string key and value pairs
        /// </summary>
        /// <param name="Issuer"></param>
        /// <param name="AssertionExpirationMinutes"></param>
        /// <param name="Audience"></param>
        /// <param name="Subject"></param>
        /// <param name="Recipient"></param>
        /// <param name="Attributes">Dictionary of string key, string value pairs</param>
        /// <returns>Assertion to sign and include in Response</returns>
        private static AssertionType CreateSAML20Assertion(string Issuer,
            int AssertionExpirationMinutes,
            string Audience,
            string Subject,
            string Recipient,
            Dictionary<string, string> Attributes)
        {
            var NewAssertion = new AssertionType
            {
                Version = "2.0",
                IssueInstant = DateTime.UtcNow,
                ID = "_" + Guid.NewGuid().ToString(),
                Issuer = new NameIDType() {Value = Issuer.Trim()}
            };

            // Create Issuer

            // Create Assertion Subject
            var subject = new SubjectType();
            var subjectNameIdentifier = new NameIDType() { Value = Subject.Trim(), Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified" };
            var subjectConfirmation = new SubjectConfirmationType() { Method = "urn:oasis:names:tc:SAML:2.0:cm:bearer", SubjectConfirmationData = new SubjectConfirmationDataType() { NotOnOrAfter = DateTime.UtcNow.AddMinutes(AssertionExpirationMinutes), Recipient = Recipient } };
            subject.Items = new object[] { subjectNameIdentifier, subjectConfirmation };
            NewAssertion.Subject = subject;

            // Create Assertion Conditions
            var conditions = new ConditionsType
            {
                NotBefore = DateTime.UtcNow,
                NotBeforeSpecified = true,
                NotOnOrAfter = DateTime.UtcNow.AddMinutes(AssertionExpirationMinutes),
                NotOnOrAfterSpecified = true,
                Items =
                    new ConditionAbstractType[]
                    {new AudienceRestrictionType() {Audience = new string[] {Audience.Trim()}}}
            };
            NewAssertion.Conditions = conditions;

            // Add AuthnStatement and Attributes as Items
            var authStatement = new AuthnStatementType() { AuthnInstant = DateTime.UtcNow, SessionIndex = NewAssertion.ID };
            var context = new AuthnContextType();
            context.ItemsElementName = new[] { ItemsChoiceType5.AuthnContextClassRef };
            context.Items = new object[] { "urn:oasis:names:tc:SAML:2.0:ac:classes:unspecified" };
            authStatement.AuthnContext = context;

            var attributeStatement = new AttributeStatementType();
            attributeStatement.Items = new AttributeType[Attributes.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> attribute in Attributes)
            {
                attributeStatement.Items[i] = new AttributeType()
                {
                    Name = attribute.Key,
                    AttributeValue = new object[] { attribute.Value },
                    NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic"
                };
                i++;
            }

            NewAssertion.Items = new StatementAbstractType[] { authStatement, attributeStatement };

            return NewAssertion;
        }
    }
}