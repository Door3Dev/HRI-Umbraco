using System;

using ComponentSpace.SAML2.Configuration;

namespace ValidateConfig {
    /// <summary>
    /// Validates the SAML configuration against its schema. 
    /// 
    /// Usage: ValidateConfig <filename>
    /// 
    /// where the file contains the SAML configuration.
    /// </summary>
    static class Program {
        private const int expectedArgCount = 1;

        private static string fileName;

        private static void ParseArguments(String[] args) {
            if (args.Length < expectedArgCount) {
                throw new ArgumentException("Wrong number of arguments.");
            }

            fileName = args[0];
        }

        private static void ShowUsage() {
            Console.Error.WriteLine("ValidateConfig <saml.config>");
        }

        static void Main(string[] args) {
            try {
                ParseArguments(args);

                Console.Error.WriteLine("Validating {0}.", fileName);
                SAMLConfiguration.Validate(fileName);
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
