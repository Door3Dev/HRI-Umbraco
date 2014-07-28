using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using ComponentSpace.SAML2.Configuration;
using ComponentSpace.SAML2.Metadata;

namespace ExportMetadata {
    /// <summary>
    /// Exports the saml.config configuration file used by the high-level API as SAML metadata.
    /// 
    /// Usage: ExportMetadata <partner-name> [<certificate-filename>] <metadata-filename>
    /// 
    /// where the file contains the SAML metadata.
    /// 
    /// The saml.config file is assumed to be in the current directory.
    /// </summary>
    class Program {
        private const string configurationFileName = "saml.config";

        private static string partnerName;
        private static string certificateFileName;
        private static string metadataFileName;

        private static void ParseArguments(String[] args) {
            if (args.Length < 2) {
                throw new ArgumentException("Wrong number of arguments.");
            }

            int index = 0;
            partnerName = args[index++];

            if (args.Length > 2) {
                certificateFileName = args[index++];
            }

            metadataFileName = args[index++];
        }

        private static void ShowUsage() {
            Console.Error.WriteLine("ExportMetadata <partner-name> [<certificate-filename>] <metadata-filename>");
            Console.Error.WriteLine("The {0} in the current directory is exported.", configurationFileName);
        }

        private static SAMLConfiguration LoadSAMLConfiguration() {
            Console.WriteLine("Loading SAML configuration file {0}.", configurationFileName);

            if (!File.Exists(configurationFileName)) {
                throw new ArgumentException(string.Format("The SAML configuration file {0} doesn't exist.", configurationFileName));
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(configurationFileName);

            return new SAMLConfiguration(xmlDocument.DocumentElement);
        }

        private static X509Certificate2 LoadCertificate() {
            Console.WriteLine("Loading X.509 certificate file {0}.", certificateFileName);

            if (!File.Exists(certificateFileName)) {
                throw new ArgumentException(string.Format("The X.509 certificate file {0} doesn't exist.", certificateFileName));
            }

            return new X509Certificate2(certificateFileName, (string)null, X509KeyStorageFlags.MachineKeySet); ;
        }

        private static void SaveMetadata(EntityDescriptor entityDescriptor) {
            Console.WriteLine("Saving SAML metadata to {0}.", metadataFileName);

            XmlDocument xmlDocument = entityDescriptor.ToXml().OwnerDocument;

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(metadataFileName, null)) {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlDocument.Save(xmlTextWriter);
            }
        }

        static void Main(string[] args) {
            try {
                ParseArguments(args);

                SAMLConfiguration samlConfiguration = LoadSAMLConfiguration();
                X509Certificate2 x509Certificate = null;

                if (!string.IsNullOrEmpty(certificateFileName)) {
                    x509Certificate = LoadCertificate();
                }

                EntityDescriptor entityDescriptor = MetadataExporter.Export(samlConfiguration, x509Certificate, partnerName);

                SaveMetadata(entityDescriptor);
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
