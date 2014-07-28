package test;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;

import javax.xml.crypto.KeySelector;
import javax.xml.crypto.dsig.XMLSignature;
import javax.xml.crypto.dsig.XMLSignatureFactory;
import javax.xml.crypto.dsig.dom.DOMValidateContext;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/**
 * This class uses the Java XML signature API to verify XML digital signatures
 * on SAML v1.1 and SAML v2.0 assertions and messages.
 * 
 * Usage: test.VerifyXMLSignature -c <certificateFileName> <fileName>
 * 
 * where
 * 
 * <certificateFileName> is the certificate file containing certificate used to verify the signature
 * <fileName> is the file containing the signed XML to verify
 */
public class VerifyXMLSignature {
	private static final int EXPECTED_ARG_COUNT = 3;
	
	/**
	 * The attributes that are explicitly registered as ID attributes.
	 * NB. The ID attribute does not have to be explicitly registered.
	 */
	private static class IDAttributeNames {
		/* SAML v1.1 assertion ID */
		public static final String ASSERTION_ID = "AssertionID";
		
		/* SAML v1.1 request ID */
		public static final String REQUEST_ID = "RequestID";
		
		/* SAML v2.0 response ID */
		public static final String RESPONSE_ID = "ResponseID";
    }
	
	private static String certificateFileName;
	private static String fileName;

	private static X509Certificate x509Certificate;

	private static void parseArguments(String[] args) throws IllegalArgumentException {
		if (args.length < EXPECTED_ARG_COUNT) {
			throw new IllegalArgumentException("Wrong number of arguments.");
		}

		if (!args[0].equals("-c")) {
			throw new IllegalArgumentException("Missing argument.");
		}
		
		certificateFileName = args[1];
		fileName = args[2];
	}

	private static void showUsage() {
		System.err.println("Usage: test.VerifyXMLSignature -c <certificateFileName> <fileName>");
	}

	private static void loadCertificate() throws Exception {
		FileInputStream fileInputStream = null;

		try {
			fileInputStream = new FileInputStream(certificateFileName);
			BufferedInputStream bufferedInputStream = new BufferedInputStream(fileInputStream);
			CertificateFactory certificateFactory = CertificateFactory.getInstance("X.509");
			x509Certificate = (X509Certificate) certificateFactory.generateCertificate(bufferedInputStream);
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
	
	private static NodeList getSignatures(Document document) {
		return document.getElementsByTagNameNS(XMLSignature.XMLNS, "Signature");
	}
	
	private static void registerID(DOMValidateContext domValidateContext, Element signatureElement) {
		Element referencedElement = (Element) signatureElement.getParentNode();
		
		if (referencedElement.hasAttribute(IDAttributeNames.ASSERTION_ID)) {
			domValidateContext.setIdAttributeNS(referencedElement, null, IDAttributeNames.ASSERTION_ID);			
		}
		
		if (referencedElement.hasAttribute(IDAttributeNames.REQUEST_ID)) {
			domValidateContext.setIdAttributeNS(referencedElement, null, IDAttributeNames.REQUEST_ID);			
		}

		if (referencedElement.hasAttribute(IDAttributeNames.RESPONSE_ID)) {
			domValidateContext.setIdAttributeNS(referencedElement, null, IDAttributeNames.RESPONSE_ID);			
		}
	}
	
	private static boolean verifySignature(Element signatureElement) throws Exception {
		XMLSignatureFactory xmlSignatureFactory = XMLSignatureFactory.getInstance();
		DOMValidateContext domValidateContext = new DOMValidateContext(KeySelector.singletonKeySelector(x509Certificate.getPublicKey()), signatureElement);
		registerID(domValidateContext, signatureElement);
		XMLSignature xmlSignature = xmlSignatureFactory.unmarshalXMLSignature(domValidateContext);
		
		return xmlSignature.validate(domValidateContext);
	}
	
	public static void main(String[] args) {
		try {
			parseArguments(args);
			
			if (certificateFileName != null) {
				System.err.println("Loading certificate: " + certificateFileName);
				loadCertificate();
			}
			
			System.err.println("Loading XML: " + fileName);
			Document document = loadXML(fileName);
			
			NodeList signatureElements = getSignatures(document);
			
			if (signatureElements.getLength() == 0) {
				System.err.println("The XML is not signed");
				
				return;				
			}
			
			for (int i = 0; i < signatureElements.getLength(); i++) {
				System.err.println("Verifying XML signature");
								
				boolean verified = verifySignature((Element) signatureElements.item(i));
				
				System.err.println("Signature verified: " + verified);
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
