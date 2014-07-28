package test;

import java.io.File;
import java.io.FileInputStream;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.util.List;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.opensaml.Configuration;
import org.opensaml.DefaultBootstrap;
import org.opensaml.common.SignableSAMLObject;
import org.opensaml.xml.XMLObject;
import org.opensaml.xml.io.Unmarshaller;
import org.opensaml.xml.security.SecurityHelper;
import org.opensaml.xml.security.keyinfo.KeyInfoHelper;
import org.opensaml.xml.security.x509.BasicX509Credential;
import org.opensaml.xml.signature.KeyInfo;
import org.opensaml.xml.signature.Signature;
import org.opensaml.xml.signature.SignatureValidator;
import org.opensaml.xml.validation.ValidationException;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/**
 * This class uses the OpenSAML 2.0 Java API to verify XML digital signatures on SAML objects.
 * 
 * Usage: test.VerifySAML [-c <certificateFileName>] <fileName>
 * 
 * where
 * 
 * <certificateFileName> is the optional certificate file containing certificate used to verify the signature
 * <fileName> is the file containing the SAML to verify
 * 
 * If a certificate file is not supplied then the certificate contained within the signature, if any, is used.
 */
public class VerifySAML {
	private static final int expectedArgCount = 1;
	
	private static class Namespaces {
		public static final String ASSERTION = "urn:oasis:names:tc:SAML:2.0:assertion";
		public static final String PROTOCOL = "urn:oasis:names:tc:SAML:2.0:protocol";		
	}
	
	private static class ElementNames {
		public static final String ASSERTION = "Assertion";
		public static final String REQUEST = "Request";
		public static final String RESPONSE = "Response";
	}
	
	private static String certificateFileName;
	private static String fileName;

	private static X509Certificate x509Certificate;

	private static void parseArguments(String[] args) throws IllegalArgumentException {
		if (args.length < expectedArgCount) {
			throw new IllegalArgumentException("Wrong number of arguments.");
		}

		if (args[0].equals("-c")) {
			certificateFileName = args[1];

			if (args.length < expectedArgCount + 2) {
				throw new IllegalArgumentException("Wrong number of arguments.");
			}

			fileName = args[2];
		} else {
			fileName = args[0];
		}
	}

	private static void showUsage() {
		System.err.println("Usage: test.VerifySAML [-c <certificateFileName>] <fileName>");
	}

	private static void loadCertificate() throws Exception {
		FileInputStream fileInputStream = null;

		try {
			fileInputStream = new FileInputStream(certificateFileName);
			CertificateFactory certificateFactory = CertificateFactory.getInstance("X.509");
			x509Certificate = (X509Certificate) certificateFactory.generateCertificate(fileInputStream);
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
	
	private static NodeList getRequestElements(Document document) {
		return document.getElementsByTagNameNS(Namespaces.PROTOCOL, ElementNames.REQUEST);
	}

	private static NodeList getResponseElements(Document document) {
		return document.getElementsByTagNameNS(Namespaces.PROTOCOL, ElementNames.RESPONSE);
	}

	private static NodeList getAssertionElements(Document document) {
		return document.getElementsByTagNameNS(Namespaces.ASSERTION, ElementNames.ASSERTION);
	}

	private static XMLObject unmarshall(Element samlElement) throws Exception {
        Unmarshaller unmarshaller = Configuration.getUnmarshallerFactory().getUnmarshaller(samlElement);
        
        if (unmarshaller == null) {
        	throw new IllegalArgumentException("Failed to unmarshall XML to SAML object.");
        }
		
		return unmarshaller.unmarshall(samlElement);
	}

	private static boolean verifySignature(Signature signature) throws Exception {
		BasicX509Credential signatureCredential = null; 
		
		if (x509Certificate != null) {
			signatureCredential = SecurityHelper.getSimpleCredential(x509Certificate, null);
		} else {
			KeyInfo keyInfo = signature.getKeyInfo();
			
			if (keyInfo != null) {
				List<X509Certificate> x509certificates = KeyInfoHelper.getCertificates(keyInfo);
				
				if (x509certificates != null && !x509certificates.isEmpty()) {
					signatureCredential = SecurityHelper.getSimpleCredential(x509certificates.get(0), null);			
				}
			}
		}
		
		if (signatureCredential == null) {
			throw new IllegalArgumentException("No X.509 certificate to verify signature.");
		}
		
		SignatureValidator signatureValidator = new SignatureValidator(signatureCredential);
		
		try {
			signatureValidator.validate(signature);
		}
		
		catch (ValidationException exception) {
			return false;
		}
		
		return true;
	}
	
	public static boolean verifySignature(Element samlElement) {
		try {
			XMLObject xmlObject = unmarshall(samlElement);
			
			if (!(xmlObject instanceof SignableSAMLObject)) {
				System.err.println(String.format("Cannot verify signature on this object type: %1$s", xmlObject.getClass().getName()));
				return false;
			}

			SignableSAMLObject signableSAMLObject = (SignableSAMLObject) xmlObject;
			
			if (!signableSAMLObject.isSigned()) {
				System.err.println(String.format("The SAML element %1$s is not signed.", samlElement.getLocalName()));
				return false;
			}
			
			System.err.println(String.format("Verifying signature on %1$s", samlElement.getLocalName()));
			boolean verified = verifySignature(signableSAMLObject.getSignature());
			
			System.err.println(String.format("Signature on %1$s verified: %2$b", samlElement.getLocalName(), verified));
		}
		
		catch (Exception exception) {
			exception.printStackTrace();
			return false;
		}
		
		return true;
	}
	
	public static void main(String[] args) {
		try {
			parseArguments(args);

			if (certificateFileName != null) {
				System.err.println("Loading certificate: " + certificateFileName);
				loadCertificate();
			}
			
			System.err.println(String.format("Loading document: %1$s", fileName));
			Document document = loadXML(fileName);
			
			System.err.println("Initializing OpenSAML");
			DefaultBootstrap.bootstrap();

			NodeList nodeList = getRequestElements(document);

			for (int i = 0; i < nodeList.getLength(); i++) {
				verifySignature((Element) nodeList.item(i));
			}
			
			nodeList = getResponseElements(document);
			
			for (int i = 0; i < nodeList.getLength(); i++) {
				verifySignature((Element) nodeList.item(i));
			}
			
			nodeList = getAssertionElements(document);

			for (int i = 0; i < nodeList.getLength(); i++) {
				verifySignature((Element) nodeList.item(i));
			}
		}
		
		catch (Exception exception) {
			exception.printStackTrace();

			if (exception instanceof IllegalArgumentException) {
				showUsage();
			}
		}
	}
}
