using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.IO;
using System.Xml;

using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;

namespace SignSAML {
    /// <summary>
    /// Decrypts SAML v2.0 assertions.
    /// 
    /// Usage: DecryptSAML [-a <algorithm>] -k <keystore> -p <password> <filename>
    /// 
    /// where the file contains a SAML assertion.
    /// 
    /// The decrypted SAML assertion is written to standard output.
    /// 
    /// See System.Security.Cryptography.Xml.EncryptedXml for algorithm types.
    /// </summary>
    static class Program {
        private const int expectedArgCount = 5;

        private static string algorithm;
        private static string keyStoreFileName;
        private static string password;
        private static string fileName;

        private static X509Certificate2 x509Certificate;

        private static void ParseArguments(String[] args) {
            if (args.Length < expectedArgCount) {
                throw new ArgumentException("Wrong number of arguments.");
            }

            int index = 0;

            if (args.Length >= expectedArgCount + 2) {
                if (args[index++] != "-a") {
                    throw new ArgumentException("Missing algorithm.");
                }

                algorithm = args[index++];
            }

            if (args[index++] != "-k") {
                throw new ArgumentException("Missing key store.");
            }

            keyStoreFileName = args[index++];

            if (args[index++] != "-p") {
                throw new ArgumentException("Missing key store password.");
            }

            password = args[index++];
            fileName = args[index++];
        }

        private static void ShowUsage() {
            Console.Error.WriteLine("Usage: DecryptSAML [-a <algorithm>] -k <keystore> -p <password> <filename>");
        }

        private static void LoadKeyAndCertificate() {
            Console.Error.WriteLine("Loading certificate and key from " + keyStoreFileName);

            if (!File.Exists(keyStoreFileName)) {
                throw new ArgumentException("The certificate file " + keyStoreFileName + " doesn't exist.");
            }

            x509Certificate = new X509Certificate2(keyStoreFileName, password);
        }

        private static XmlDocument LoadXmlDocument() {
            Console.Error.WriteLine("Loading " + fileName);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(fileName);

            return xmlDocument;
        }

        private static XmlElement DecryptAssertion(XmlElement xmlElement) {
            Console.Error.WriteLine("Decrypting SAML assertion");

            EncryptedAssertion encryptedAssertion = new EncryptedAssertion(xmlElement);

            EncryptionMethod encryptionMethod = null;

            if (!String.IsNullOrEmpty(algorithm)) {
                encryptionMethod = new EncryptionMethod(algorithm);
            }

            return encryptedAssertion.DecryptToXml(x509Certificate.PrivateKey, null, encryptionMethod);
        }

        static void Main(string[] args) {
            try {
                ParseArguments(args);
                LoadKeyAndCertificate();

                XmlDocument xmlDocument = LoadXmlDocument();

                Console.WriteLine(DecryptAssertion(xmlDocument.DocumentElement).OuterXml);
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
