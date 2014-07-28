using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using ComponentSpace.SAML2.Configuration;
using ComponentSpace.SAML2.Metadata;

namespace ImportMetadata {
    /// <summary>
    /// Imports SAML metadata into the saml.config configuration file used by the high-level API.
    /// 
    /// Usage: ImportMetadata <metadata-filename>
    /// 
    /// where the file contains the SAML metadata.
    /// 
    /// The saml.config file, if any, is assumed to be in the current directory.
    /// </summary>
    class Program {
        private const string configurationFileName = "saml.config";

        private const string promptMessage = "TODO: value required";

        private static string fileName;

        private static void ParseArguments(String[] args) {
            if (args.Length < 1) {
                throw new ArgumentException("Wrong number of arguments.");
            }

            fileName = args[0];
        }

        private static void ShowUsage() {
            Console.Error.WriteLine("ImportMetadata <metadata-filename>");
            Console.Error.WriteLine("The {0} in the current directory is created or updated.", configurationFileName);
        }

        private static SAMLConfiguration LoadSAMLConfiguration() {
            SAMLConfiguration samlConfiguration = null;

            if (File.Exists(configurationFileName)) {
                Console.WriteLine("Loading SAML configuration file {0}.", configurationFileName);

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.PreserveWhitespace = true;
                xmlDocument.Load(configurationFileName);

                samlConfiguration = new SAMLConfiguration(xmlDocument.DocumentElement);
            } else {
                samlConfiguration = new SAMLConfiguration();
            }

            return samlConfiguration;
        }

        private static EntitiesDescriptor LoadMetadata() {
            Console.WriteLine("Loading SAML metadata file {0}.", fileName);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(fileName);

            EntitiesDescriptor entitiesDescriptor = null;

            if (EntitiesDescriptor.IsValid(xmlDocument.DocumentElement)) {
                Console.WriteLine("Reading SAML entities descriptor metadata.");
                entitiesDescriptor = new EntitiesDescriptor(xmlDocument.DocumentElement);
            } else if (EntityDescriptor.IsValid(xmlDocument.DocumentElement)) {
                Console.WriteLine("Reading SAML entity descriptor metadata.");
                entitiesDescriptor = new EntitiesDescriptor();
                entitiesDescriptor.EntityDescriptors.Add(new EntityDescriptor(xmlDocument.DocumentElement));
            } else {
                throw new ArgumentException("Expecting entities descriptor or entity descriptor.");
            }

            return entitiesDescriptor;
        }

        private static void AddLocalProviders(SAMLConfiguration samlConfiguration) {
            if (samlConfiguration.PartnerIdentityProviderConfigurations.Count == 0 || samlConfiguration.PartnerServiceProviderConfigurations.Count > 0) {
                samlConfiguration.IdentityProviderConfiguration = new IdentityProviderConfiguration();
                samlConfiguration.IdentityProviderConfiguration.Name = promptMessage;
                samlConfiguration.IdentityProviderConfiguration.CertificateFile = promptMessage;
            }

            if (samlConfiguration.PartnerServiceProviderConfigurations.Count == 0 || samlConfiguration.PartnerIdentityProviderConfigurations.Count > 0) {
                samlConfiguration.ServiceProviderConfiguration = new ServiceProviderConfiguration();
                samlConfiguration.ServiceProviderConfiguration.Name = promptMessage;
                samlConfiguration.ServiceProviderConfiguration.CertificateFile = promptMessage;
                samlConfiguration.ServiceProviderConfiguration.AssertionConsumerServiceUrl = promptMessage;
            }
        }

        private static void UpdatePartnerProviders(SAMLConfiguration samlConfiguration) {
            foreach (PartnerIdentityProviderConfiguration partnerIdentityProviderConfiguration in samlConfiguration.PartnerIdentityProviderConfigurations.Values) {
                partnerIdentityProviderConfiguration.CertificateFile = promptMessage;
            }

            foreach (PartnerServiceProviderConfiguration partnerServiceProviderConfiguration in samlConfiguration.PartnerServiceProviderConfigurations.Values) {
                partnerServiceProviderConfiguration.CertificateFile = promptMessage;
            }
        }

        private static void SaveSAMLConfiguration(SAMLConfiguration samlConfiguration) {
            Console.WriteLine("Saving SAML configuration to {0}.", configurationFileName);

            XmlDocument xmlDocument = samlConfiguration.ToXml().OwnerDocument;

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(configurationFileName, null)) {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlDocument.Save(xmlTextWriter);
            }
        }

        private static string CreateFileName(X509Certificate2 x509Certificate) {
            string subjectName = Regex.Replace(x509Certificate.Subject, "CN=", "", RegexOptions.IgnoreCase);

            return string.Format("{0}.cer", Regex.Replace(subjectName, @"[^A-Za-z0-9_=\.]+", "_"));
        }

        private static void SaveCertificate(X509Certificate2 x509Certificate) {
            string fileName = CreateFileName(x509Certificate);

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
            stringBuilder.AppendLine(Convert.ToBase64String(x509Certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            stringBuilder.AppendLine("-----END CERTIFICATE-----");

            Console.WriteLine("Saving X.509 certificate to {0}.", fileName);
            File.WriteAllText(fileName, stringBuilder.ToString());
        }

        private static void SaveCertificates(IList<X509Certificate2> x509Certificates) {
            foreach (X509Certificate2 x509Certificate in x509Certificates) {
                SaveCertificate(x509Certificate);
            }
        }

        static void Main(string[] args) {
            try {
                ParseArguments(args);

                EntitiesDescriptor entitiesDescriptor = LoadMetadata();

                SAMLConfiguration samlConfiguration = LoadSAMLConfiguration();
                IList<X509Certificate2> x509Certificates = new List<X509Certificate2>();

                MetadataImporter.Import(entitiesDescriptor, samlConfiguration, x509Certificates);
                AddLocalProviders(samlConfiguration);
                UpdatePartnerProviders(samlConfiguration);

                SaveSAMLConfiguration(samlConfiguration);
                SaveCertificates(x509Certificates);
            }

            catch (Exception exception) {
                Console.Error.WriteLine(exception.ToString());

                if (exception is ArgumentException) {
                    ShowUsage();
                }
            }
        }
    }
}
