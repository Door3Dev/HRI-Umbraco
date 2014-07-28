package test;

import java.io.File;
import java.io.FileInputStream;
import java.io.StringWriter;
import java.security.KeyStore;
import java.security.PrivateKey;
import java.security.cert.X509Certificate;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.opensaml.Configuration;
import org.opensaml.DefaultBootstrap;
import org.opensaml.common.SignableSAMLObject;
import org.opensaml.xml.XMLObject;
import org.opensaml.xml.io.Marshaller;
import org.opensaml.xml.io.Unmarshaller;
import org.opensaml.xml.security.SecurityHelper;
import org.opensaml.xml.security.keyinfo.KeyInfoGenerator;
import org.opensaml.xml.security.x509.BasicX509Credential;
import org.opensaml.xml.signature.Signature;
import org.opensaml.xml.signature.SignatureConstants;
import org.opensaml.xml.signature.Signer;
import org.opensaml.xml.signature.impl.SignatureBuilder;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

/**
 * This class uses the OpenSAML 2.0 Java API to sign SAML objects using XML digital signatures.
 * 
 * Usage: test.SignSAML -k <keystoreFileName> -p <password> -a <alias> <fileName>
 * 
 * where
 * 
 * <keystoreFileName> is the key store file containing the private key and certificate used to generate the signature
 * <password> is the password to the key store file
 * <alias> identifies the private key and certificate in the key store
 * <fileName> is the file containing the SAML to sign
 * 
 * The signed SAML is written to standard output.
 */
public class SignSAML {
	private static final int expectedArgCount = 7;
	
	private static String keyStoreFileName;
	private static char[] password;
	private static String alias;
	private static String fileName;

	private static PrivateKey privateKey;
	private static X509Certificate x509Certificate;

	private static void parseArguments(String[] args) throws IllegalArgumentException {
		if (args.length < expectedArgCount) {
			throw new IllegalArgumentException("Wrong number of arguments.");
		}

		if (!args[0].equals("-k")) {
			throw new IllegalArgumentException("Missing key store.");
		}

		keyStoreFileName = args[1];

		if (!args[2].equals("-p")) {
			throw new IllegalArgumentException("Missing key store password.");
		}

		password = args[3].toCharArray();

		if (!args[4].equals("-a")) {
			throw new IllegalArgumentException("Missing key alias.");
		}

		alias = args[5];

		fileName = args[6];
	}

	private static void showUsage() {
		System.err.println("Usage: test.SignSAML -k <keystoreFileName> -p <password> -a <alias> <fileName>");
	}

	private static void loadKeyAndCertificate() throws Exception {
		FileInputStream fileInputStream = null;

		try {
			fileInputStream = new FileInputStream(keyStoreFileName);
			KeyStore keyStore = KeyStore.getInstance("JKS");
			keyStore.load(fileInputStream, password);
		
			privateKey = (PrivateKey) keyStore.getKey(alias, password);
			x509Certificate = (X509Certificate) keyStore.getCertificate(alias);
		}

		finally {
			if (fileInputStream != null) {
				fileInputStream.close();
			}
		}
	}

	private static Document loadXML(String fileName) throws Exception  {
		DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
		documentBuilderFactory.setNamespaceAware(true);
		DocumentBuilder documentBuilder = documentBuilderFactory.newDocumentBuilder();
		
		return documentBuilder.parse(new File(fileName));
	}
	
	private static XMLObject unmarshall(Element samlElement) throws Exception {
        Unmarshaller unmarshaller = Configuration.getUnmarshallerFactory().getUnmarshaller(samlElement);
        
        if (unmarshaller == null) {
        	throw new IllegalArgumentException("Failed to unmarshall XML to SAML object.");
        }
		
		return unmarshaller.unmarshall(samlElement);
	}

	private static Element marshall(XMLObject xmlObject) throws Exception {
		Marshaller marshaller = Configuration.getMarshallerFactory().getMarshaller(xmlObject);
        
		if (marshaller == null) {
        	throw new IllegalArgumentException("Failed to marshall SAML object to XML.");
        }
		
		return marshaller.marshall(xmlObject);
	}
	
	private static void sign(SignableSAMLObject signableSAMLObject) throws Exception {
		BasicX509Credential signatureCredential = SecurityHelper.getSimpleCredential(x509Certificate, privateKey);
		
		SignatureBuilder signatureBuilder = (SignatureBuilder) Configuration.getBuilderFactory().getBuilder(Signature.DEFAULT_ELEMENT_NAME);
		Signature signature = signatureBuilder.buildObject(Signature.DEFAULT_ELEMENT_NAME);
		
		signature.setSigningCredential(signatureCredential);
        signature.setSignatureAlgorithm(SignatureConstants.ALGO_ID_SIGNATURE_RSA);
		signature.setCanonicalizationAlgorithm(SignatureConstants.ALGO_ID_C14N_EXCL_OMIT_COMMENTS);
		
        KeyInfoGenerator keyInfoGenerator = SecurityHelper.getKeyInfoGenerator(signatureCredential, null, null);
		signature.setKeyInfo(keyInfoGenerator.generate(signatureCredential));
		
		signableSAMLObject.setSignature(signature);
		
		Marshaller marshaller = Configuration.getMarshallerFactory().getMarshaller(signableSAMLObject);
		marshaller.marshall(signableSAMLObject);
		
		Signer.signObject(signature);		
	}
	
	private static String elementToString(Element element) throws Exception {
		TransformerFactory transformerFactory = TransformerFactory.newInstance();
		Transformer transformer = transformerFactory.newTransformer();
		StringWriter stringWriter = new StringWriter();
		transformer.transform(new DOMSource(element), new StreamResult(stringWriter));
		
		return stringWriter.toString();		
	}
	
	public static void main(String[] args) {
		try {
			parseArguments(args);

			System.err.println("Loading key store: " + keyStoreFileName);
			loadKeyAndCertificate();
			
			System.err.println("Loading SAML: " + fileName);
			Document document = loadXML(fileName);
			Element samlElement = document.getDocumentElement();
			
			System.err.println("Initializing OpenSAML");
			DefaultBootstrap.bootstrap();
			
			XMLObject xmlObject = unmarshall(samlElement);
			
			if (!(xmlObject instanceof SignableSAMLObject)) {
				System.err.println("Cannot sign this object type: " + xmlObject.getClass().getName());
				return;
			}
			
			System.err.println("Signing SAML");
			sign((SignableSAMLObject) xmlObject);
			
			samlElement = marshall(xmlObject);
			System.out.println(elementToString(samlElement));
		}
		
		catch (Exception exception) {
			exception.printStackTrace();

			if (exception instanceof IllegalArgumentException) {
				showUsage();
			}
		}
	}
}
