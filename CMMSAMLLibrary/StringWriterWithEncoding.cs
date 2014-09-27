using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoverMyMeds.SAML.Library
{
    public sealed class StringWriterWithEncoding : StringWriter
    {

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
