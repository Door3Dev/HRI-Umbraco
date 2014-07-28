'
' This example demonstrates the following:
'
'   1. Creating a SAML response message and accessing its contents
'   2. Converting a SAML response message to and from XML
'   3. Signing a SAML response message and verifying the signature
'   4. Creating a SAML assertion and accessing its contents
'   5. Converting a SAML assertion to and from XML
'   6. Signing a SAML assertion and verifying the signature
'   7. Encrypting and decrypting a SAML assertion
'   8. Encrypting and decrypting a SAML attribute
'

Imports System.IO
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Cryptography.Xml
Imports System.Xml

Imports ComponentSpace.SAML2
Imports ComponentSpace.SAML2.Assertions
Imports ComponentSpace.SAML2.Protocols
Imports ComponentSpace.SAML2.Utility

Module MainModule

    ' The test certificate/private key file name - must be in the current directory
    Private Const testCertificateFileName As String = "idp.pfx"

    ' The test certificate/private key file password
    Private Const testCertificateFilePassword As String = "password"

    ' The certificate and private key to use during XML signature generation and encryption
    Private x509Certificate As X509Certificate2

    '
    ' Loads the test certificate and private key from the PFX file
    '
    Private Sub LoadTestCertificateAndKey()
        If Not File.Exists(testCertificateFileName) Then
            Throw New ArgumentException("The certificate file " + testCertificateFileName + " doesn't exist.")
        End If

        x509Certificate = New X509Certificate2(testCertificateFileName, testCertificateFilePassword)
    End Sub

    '
    ' Signs the SAML assertion
    '
    Private Sub SignAssertion(ByVal xmlSAMLAssertion As XmlElement)
        SAMLAssertionSignature.Generate(xmlSAMLAssertion, x509Certificate.PrivateKey, x509Certificate)
    End Sub

    ' Verifies the SAML assertion signature 
    Private Function VerifyAssertionSignature(ByVal xmlSAMLAssertion As XmlElement) As Boolean
        If SAMLAssertionSignature.IsSigned(xmlSAMLAssertion) Then
            VerifyAssertionSignature = SAMLAssertionSignature.Verify(xmlSAMLAssertion)
        Else
            VerifyAssertionSignature = True
        End If
    End Function

    ' Encrypts the SAML assertion
    Private Function EncryptAssertion(ByVal xmlSAMLAssertion As XmlElement) As EncryptedAssertion
        EncryptAssertion = New EncryptedAssertion(xmlSAMLAssertion, x509Certificate, New EncryptionMethod(EncryptedXml.XmlEncRSAOAEPUrl), New EncryptionMethod(EncryptedXml.XmlEncAES256Url))
        'EncryptAssertion = New EncryptedAssertion(xmlSAMLAssertion, x509Certificate, New EncryptionMethod(EncryptedXml.XmlEncRSA15Url), New EncryptionMethod(EncryptedXml.XmlEncTripleDESUrl))
    End Function

    ' Decrypts the SAML assertion
    Private Function DecryptAssertion(ByVal encryptedAssertion As EncryptedAssertion) As XmlElement
        DecryptAssertion = encryptedAssertion.DecryptToXml(x509Certificate.PrivateKey, Nothing, Nothing)
    End Function

    ' Encrypts the SAML attribute
    Private Function EncryptAttribute(ByVal samlAttribute As SAMLAttribute) As EncryptedAttribute
        EncryptAttribute = New EncryptedAttribute(samlAttribute, x509Certificate, New EncryptionMethod(EncryptedXml.XmlEncAES256Url))
    End Function

    ' Decrypts the SAML attribute
    Private Function DecryptAttribute(ByVal encryptedAttribute As EncryptedAttribute) As SAMLAttribute
        DecryptAttribute = encryptedAttribute.Decrypt(x509Certificate.PrivateKey, Nothing, Nothing)
    End Function

    '
    ' Signs the SAML response
    '
    Private Sub SignResponse(ByVal xmlSAMLResponse As XmlElement)
        SAMLMessageSignature.Generate(xmlSAMLResponse, x509Certificate.PrivateKey, x509Certificate)
    End Sub

    ' Verifies the SAML response signature 
    Private Function VerifyResponseSignature(ByVal xmlSAMLResponse As XmlElement) As Boolean
        If SAMLMessageSignature.IsSigned(xmlSAMLResponse) Then
            VerifyResponseSignature = SAMLMessageSignature.Verify(xmlSAMLResponse)
        Else
            VerifyResponseSignature = True
        End If
    End Function

    '
    ' Creates a SAML assertion including authentication and attribute statements
    '
    Private Function CreateAssertion() As SAMLAssertion
        Dim samlAssertion As New SAMLAssertion()

        samlAssertion.Issuer = New Issuer("http://idp.example.org", Nothing, Nothing, SAMLIdentifiers.NameIdentifierFormats.Entity, Nothing)
        samlAssertion.Subject = New Subject(New NameID("j.doe@example.com", Nothing, Nothing, SAMLIdentifiers.NameIdentifierFormats.EmailAddress, Nothing))
        samlAssertion.Conditions = New Conditions(New TimeSpan(1, 0, 0))

        Dim authnStatement As New AuthnStatement()
        authnStatement.AuthnContext = New AuthnContext()
        authnStatement.AuthnContext.AuthnContextClassRef = New AuthnContextClassRef(SAMLIdentifiers.AuthnContextClasses.InternetProtocolPassword)

        samlAssertion.Statements.Add(authnStatement)

        ' Simple attribute - maximum control of attribute creation.
        Dim attributeStatement As New AttributeStatement()
        attributeStatement.Attributes.Add(New SAMLAttribute("GivenName", SAMLIdentifiers.AttributeNameFormats.Basic, Nothing, "John"))

        samlAssertion.Statements.Add(attributeStatement)

        ' Simple attribute - using convenience method and accepting defaults for other values.
        samlAssertion.SetAttributeValue("LastName", "Doe")

        ' Simple attributes - using convenience method and accepting defaults for other values.
        samlAssertion.SetAttributeValues(New String() {"Attribute1", "Attribute2", "Attribute3"}, New String() {"Value1", "Value2", "Value3"})

        ' Attribute including attribute type - maximum control of attribute creation.
        attributeStatement = New AttributeStatement()
        attributeStatement.Attributes.Add(New SAMLAttribute("Membership", SAMLIdentifiers.AttributeNameFormats.Basic, Nothing, _
                                          XmlSchema.GetQualifiedSimpleType(XmlSchema.SimpleTypes.String), "Gold"))

        samlAssertion.Statements.Add(attributeStatement)

        ' Attribute including attribute type - using convenience method and accepting defaults for other values.
        samlAssertion.SetAttributeValue("MembershipNumber", XmlSchema.GetQualifiedSimpleType(XmlSchema.SimpleTypes.Long), 12345678)

        ' Encrypted attributes.
        attributeStatement = New AttributeStatement()
        attributeStatement.Attributes.Add(EncryptAttribute(New SAMLAttribute("Secret", SAMLIdentifiers.AttributeNameFormats.Basic, Nothing, "1234")))

        samlAssertion.Statements.Add(attributeStatement)

        attributeStatement = New AttributeStatement()
        attributeStatement.Attributes.Add(EncryptAttribute(New SAMLAttribute("TopSecret", SAMLIdentifiers.AttributeNameFormats.Basic, Nothing, "5678")))

        samlAssertion.Statements.Add(attributeStatement)

        ' Include an attribute containing XML as its value.
        ' To correctly serialize/deserialize the value as XML rather than a string, 
        ' the correct serializer must be registered for the attribute.
        ' In this example the Address attribute is XML.
        SAMLAttribute.RegisterAttributeValueSerializer("Address", Nothing, New XmlAttributeValueSerializer())

        Dim addressAttribute As New SAMLAttribute("Address", SAMLIdentifiers.AttributeNameFormats.Unspecified, Nothing)

        Dim xmlDocument As New XmlDocument()
        xmlDocument.PreserveWhitespace = True
        xmlDocument.LoadXml("<Address><Street>1234 Main Street</Street><Town>Any Town</Town><Zip>56789</Zip></Address>")

        addressAttribute.Values.Add(New AttributeValue(xmlDocument.DocumentElement))

        attributeStatement = New AttributeStatement()
        attributeStatement.Attributes.Add(addressAttribute)

        samlAssertion.Statements.Add(attributeStatement)

        CreateAssertion = samlAssertion
    End Function

    '
    ' Processes the SAML assertion
    '
    Private Sub ProcessAssertion(ByVal samlAssertion As SAMLAssertion)
        If Not samlAssertion.Issuer Is Nothing Then
            Console.WriteLine("Issuer: " + samlAssertion.Issuer.NameIdentifier)
        End If

        If Not samlAssertion.Conditions Is Nothing And Not samlAssertion.Conditions.IsWithinTimePeriod Then
            Console.WriteLine("Assertion is no longer valid")
            Exit Sub
        End If

        If Not samlAssertion.Subject Is Nothing And Not samlAssertion.Subject.NameID Is Nothing Then
            Console.WriteLine("Subject name ID: " + samlAssertion.Subject.NameID.NameIdentifier)
        End If

        ' Access authentication statements
        For Each authnStatement As AuthnStatement In samlAssertion.GetAuthenticationStatements()
            If Not authnStatement.AuthnContext Is Nothing Then
                Console.WriteLine("Authentication context: " + authnStatement.AuthnContext.AuthnContextClassRef.URI)
            End If
        Next

        ' Access attribute statements
        For Each attributeStatement As AttributeStatement In samlAssertion.GetAttributeStatements()
            For Each samlAttribute As SAMLAttribute In attributeStatement.GetUnencryptedAttributes()
                Console.WriteLine("Attribute name: " + samlAttribute.Name)

                For Each attributeValue As AttributeValue In samlAttribute.Values
                    If Not String.IsNullOrEmpty(attributeValue.Type) Then
                        Console.WriteLine("Attribute type: " + attributeValue.Type)
                    End If

                    If TypeOf attributeValue.Data Is XmlElement Then
                        Console.WriteLine("Attribute XML value: " + DirectCast(attributeValue.Data, XmlElement).OuterXml)
                    Else
                        Console.WriteLine("Attribute value: " + attributeValue.ToString())
                    End If
                Next
            Next
            For Each encryptedAttribute As EncryptedAttribute In attributeStatement.GetEncryptedAttributes()
                Dim samlAttribute As SAMLAttribute = DecryptAttribute(encryptedAttribute)

                Console.WriteLine("Attribute name: " + SAMLAttribute.Name)

                For Each attributeValue As AttributeValue In samlAttribute.Values
                    If Not String.IsNullOrEmpty(attributeValue.Type) Then
                        Console.WriteLine("Attribute type: " + attributeValue.Type)
                    End If

                    If TypeOf attributeValue.Data Is XmlElement Then
                        Console.WriteLine("Attribute XML value: " + DirectCast(attributeValue.Data, XmlElement).OuterXml)
                    Else
                        Console.WriteLine("Attribute value: " + attributeValue.ToString())
                    End If
                Next
            Next
        Next

        ' Attributes may also be accessed directly by name if there's only one attribute with the given name,
        ' the attribute only has one value, and the value is a string.
        Console.WriteLine(String.Format("Attribute {0}: {1}", "GivenName", samlAssertion.GetAttributeValue("GivenName")))
    End Sub

    '
    ' Creates a SAML response message containing a SAML assertion, a signed SAML assertion, and an encrypted SAML assertion
    '
    Private Function CreateResponse() As XmlElement
        ' Create a SAML assertion
        Console.WriteLine("Creating SAML assertion")
        Dim samlAssertion As SAMLAssertion = CreateAssertion()

        ' Convert the SAML assertion to XML
        Dim xmlSAMLAssertion As XmlElement = samlAssertion.ToXml()

        Console.WriteLine(xmlSAMLAssertion.OuterXml)

        ' Sign the SAML assertion
        Console.WriteLine("Signing SAML assertion")
        SignAssertion(xmlSAMLAssertion)

        Console.WriteLine(xmlSAMLAssertion.OuterXml)

        ' Encrypt the SAML assertion
        Console.WriteLine("Encrypting signed SAML assertion")
        Dim encryptedAssertion As EncryptedAssertion = EncryptAssertion(xmlSAMLAssertion)

        Console.WriteLine(encryptedAssertion.ToXml().OuterXml)

        ' Create a SAML response containing the SAML assertion, the signed SAML assertion, and the encrypted SAML assertion
        Console.WriteLine("Creating SAML response")
        Dim samlResponse As New SAMLResponse()

        samlResponse.Issuer = New Issuer("http://idp.example.org")
        samlResponse.Status = New Status(SAMLIdentifiers.PrimaryStatusCodes.Success, Nothing)
        samlResponse.Assertions.Add(samlAssertion)
        samlResponse.Assertions.Add(xmlSAMLAssertion)
        samlResponse.Assertions.Add(encryptedAssertion)

        ' Convert the SAML response to XML
        Dim xmlSAMLResponse As XmlElement = samlResponse.ToXml()

        Console.WriteLine(xmlSAMLResponse.OuterXml)

        ' Sign the SAML response
        Console.WriteLine("Signing SAML response")
        SignResponse(xmlSAMLResponse)

        Console.WriteLine(xmlSAMLResponse.OuterXml)

        CreateResponse = xmlSAMLResponse
    End Function

    '
    ' Processes the signed SAML assertion
    '
    Private Sub ProcessSignedAssertion(ByVal xmlSAMLAssertion As XmlElement)
        ' Verify the SAML assertion signature
        Console.WriteLine("Verifying signed SAML assertion signature")
        Console.WriteLine(xmlSAMLAssertion.OuterXml)

        If Not VerifyAssertionSignature(xmlSAMLAssertion) Then
            Console.WriteLine("Failed to verify signature")
            Exit Sub
        Else
            Console.WriteLine("Signature verified")
        End If

        ' Load the SAML assertion
        Console.WriteLine("Accessing SAML assertion contents")
        Dim samlAssertion = New SAMLAssertion(xmlSAMLAssertion)

        ' Process the SAML assertion
        ProcessAssertion(samlAssertion)
    End Sub

    '
    ' Processes the encrypted SAML assertion
    '
    Private Sub ProcessEncryptedAssertion(ByVal encryptedAssertion As EncryptedAssertion)
        ' Decrypt the SAML assertion
        Console.WriteLine("Decrypting signed SAML assertion")
        Dim xmlSAMLAssertion As XmlElement = DecryptAssertion(encryptedAssertion)

        Console.WriteLine(xmlSAMLAssertion.OuterXml)

        ' Verify the SAML assertion signature
        Console.WriteLine("Verifying signed SAML assertion signature")

        If Not VerifyAssertionSignature(xmlSAMLAssertion) Then
            Console.WriteLine("Failed to verify signature")
            Exit Sub
        Else
            Console.WriteLine("Signature verified")
        End If

        ' Load the SAML assertion
        Console.WriteLine("Accessing SAML assertion contents")
        Dim samlAssertion = New SAMLAssertion(xmlSAMLAssertion)

        ' Process the SAML assertion
        ProcessAssertion(samlAssertion)
    End Sub

    '
    ' Processes the SAML response
    '
    Private Sub ProcessResponse(ByVal xmlSAMLResponse As XmlElement)
        ' Verify the SAML response signature
        Console.WriteLine("Verifying signed SAML response signature")
        Console.WriteLine(xmlSAMLResponse.OuterXml)

        If Not VerifyResponseSignature(xmlSAMLResponse) Then
            Console.WriteLine("Failed to verify signature")
            Exit Sub
        Else
            Console.WriteLine("Signature verified")
        End If

        ' Load the SAML response and extract its contents
        Console.WriteLine("Accessing SAML response contents")
        Dim samlResponse = New SAMLResponse(xmlSAMLResponse)

        ' Check for success
        If Not samlResponse.IsSuccess() Then
            Console.WriteLine("SAML response returned error")
            Exit Sub
        End If

        ' Process the SAML assertions
        For Each samlAssertion As SAMLAssertion In samlResponse.GetAssertions
            ProcessAssertion(samlAssertion)
        Next

        ' Process the signed SAML assertions
        For Each xmlSAMLAssertion As XmlElement In samlResponse.GetSignedAssertions
            ProcessSignedAssertion(xmlSAMLAssertion)
        Next

        ' Process the encrypted SAML assertions
        For Each encryptedAssertion As EncryptedAssertion In samlResponse.GetEncryptedAssertions
            ProcessEncryptedAssertion(encryptedAssertion)
        Next
    End Sub

    Sub Main()
        Try
            ' Load the test certificate and key
            LoadTestCertificateAndKey()

            ' Create a SAML response containing a SAML assertion, a signed SAML assertion, and an encrypted SAML assertion
            Dim xmlSAMLResponse As XmlElement = CreateResponse()

            ' Process the SAML response
            ProcessResponse(xmlSAMLResponse)

        Catch ex As Exception
            Console.WriteLine(ex.ToString())
        End Try
    End Sub

End Module
