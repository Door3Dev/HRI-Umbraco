using System;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Xml;

namespace CoverMyMeds.SAML.Library
{
    public class PrefixedSignedXml : SignedXml
    {
        public PrefixedSignedXml(XmlDocument document)
            : base(document)
        { }

        public PrefixedSignedXml(XmlElement element)
            : base(element)
        { }

        public PrefixedSignedXml()
            : base()
        { }

        public void ComputeSignature(string prefix)
        {
            BuildDigestedReferences();
            AsymmetricAlgorithm signingKey = SigningKey;
            if (signingKey == null)
            {
                throw new CryptographicException("Cryptography_Xml_LoadKeyFailed");
            }
            if (SignedInfo.SignatureMethod == null)
            {
                if (!(signingKey is DSA))
                {
                    if (!(signingKey is RSA))
                    {
                        throw new CryptographicException("Cryptography_Xml_CreatedKeyFailed");
                    }
                    if (SignedInfo.SignatureMethod == null)
                    {
                        SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                    }
                }
                else
                {
                    SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
                }
            }
            var description = CryptoConfig.CreateFromName(SignedInfo.SignatureMethod) as SignatureDescription;
            if (description == null)
            {
                throw new CryptographicException("Cryptography_Xml_SignatureDescriptionNotCreated");
            }
            var hash = description.CreateDigest();
            if (hash == null)
            {
                throw new CryptographicException("Cryptography_Xml_CreateHashAlgorithmFailed");
            }
            GetC14NDigest(hash, prefix);
            m_signature.SignatureValue = description.CreateFormatter(signingKey).CreateSignature(hash);
        }

        public XmlElement GetXml(string prefix)
        {
            XmlElement e = GetXml();
            SetPrefix(prefix, e);
            return e;
        }

        //Invocar por reflexión al método privado SignedXml.BuildDigestedReferences
        private void BuildDigestedReferences()
        {
            Type t = typeof(SignedXml);
            MethodInfo m = t.GetMethod("BuildDigestedReferences", BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(this, new object[] { });
        }

        private byte[] GetC14NDigest(HashAlgorithm hash, string prefix)
        {
            //string securityUrl = (this.m_containingDocument == null) ? null : this.m_containingDocument.BaseURI;
            //XmlResolver xmlResolver = new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            XmlElement e = this.SignedInfo.GetXml();
            document.AppendChild(document.ImportNode(e, true));
            //CanonicalXmlNodeList namespaces = (this.m_context == null) ? null : Utils.GetPropagatedAttributes(this.m_context);
            //Utils.AddNamespaces(document.DocumentElement, namespaces);

            Transform canonicalizationMethodObject = this.SignedInfo.CanonicalizationMethodObject;
            //canonicalizationMethodObject.Resolver = xmlResolver;
            //canonicalizationMethodObject.BaseURI = securityUrl;
            SetPrefix(prefix, document.DocumentElement); //establecemos el prefijo antes de se que calcule el hash (o de lo contrario la firma no será válida)
            canonicalizationMethodObject.LoadInput(document);
            return canonicalizationMethodObject.GetDigestedOutput(hash);
        }

        private void SetPrefix(string prefix, XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
                SetPrefix(prefix, n);
            node.Prefix = prefix;
        }
    }
}