package test;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.StringWriter;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;

import javax.servlet.http.HttpServletRequest;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.opensaml.Configuration;
import org.opensaml.DefaultBootstrap;
import org.opensaml.common.SignableSAMLObject;
import org.opensaml.common.binding.BasicSAMLMessageContext;
import org.opensaml.saml2.binding.decoding.HTTPRedirectDeflateDecoder;
import org.opensaml.saml2.binding.security.SAML2HTTPRedirectDeflateSignatureRule;
import org.opensaml.saml2.core.NameID;
import org.opensaml.saml2.metadata.SPSSODescriptor;
import org.opensaml.util.URLBuilder;
import org.opensaml.ws.message.MessageContext;
import org.opensaml.ws.transport.http.HttpServletRequestAdapter;
import org.opensaml.xml.XMLObject;
import org.opensaml.xml.io.Marshaller;
import org.opensaml.xml.security.SecurityHelper;
import org.opensaml.xml.security.SecurityTestHelper;
import org.opensaml.xml.security.credential.StaticCredentialResolver;
import org.opensaml.xml.security.keyinfo.KeyInfoCredentialResolver;
import org.opensaml.xml.security.x509.BasicX509Credential;
import org.opensaml.xml.signature.impl.ExplicitKeySignatureTrustEngine;
import org.opensaml.xml.util.Pair;
import org.springframework.mock.web.MockHttpServletRequest;
import org.w3c.dom.Element;

/**
 * This class uses the OpenSAML 2.0 Java API to parse the HTTP Redirect URL, checking the signature, 
 * and extracting the SAML message and relay states.
 * 
 * Usage: test.ParseHttpRedirectUrl [-c <certificateFileName>] <fileName>
 * 
 * where
 * 
 * <certificateFileName> is the optional certificate file containing certificate used to verify the signature
 * <fileName> is the file containing the URL
 */
public class ParseHttpRedirectUrl {
	private static final int expectedArgCount = 1;
	
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
		System.err.println("Usage: test.ParseHttpRedirectUrl [-c <certificateFileName>] <fileName>");
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

	private static boolean isSigned(String url) {
		return url.contains("Signature=");		
	}
	
	private static HttpServletRequest createHttpServletRequest(String url) {
		MockHttpServletRequest httpServletRequest = new MockHttpServletRequest();
		httpServletRequest.setMethod("GET");
		
		URLBuilder urlBuilder = new URLBuilder(url);
		httpServletRequest.setRequestURI(urlBuilder.getPath());
		httpServletRequest.setQueryString(urlBuilder.buildQueryString());
		
        for (Pair<String, String> param : urlBuilder.getQueryParams()) {
        	httpServletRequest.setParameter(param.getFirst(), param.getSecond());
        }
        
        return httpServletRequest;
	}
	
	private static BasicSAMLMessageContext<SignableSAMLObject, SignableSAMLObject, NameID> createMessageContext(HttpServletRequest httpServletRequest) {
        BasicSAMLMessageContext<SignableSAMLObject, SignableSAMLObject, NameID> messageContext = new BasicSAMLMessageContext<SignableSAMLObject, SignableSAMLObject, NameID>();
        messageContext.setInboundMessageIssuer("www.opensaml.org");
        messageContext.setPeerEntityRole(SPSSODescriptor.DEFAULT_ELEMENT_NAME);
        messageContext.setInboundMessageTransport(new HttpServletRequestAdapter(httpServletRequest));

        return messageContext;
	}
	
	private static void verifySignature(MessageContext messageContext) throws Exception {
		BasicX509Credential signatureCredential = SecurityHelper.getSimpleCredential(x509Certificate, null);			
		StaticCredentialResolver staticCredentialResolver = new StaticCredentialResolver(signatureCredential);
		KeyInfoCredentialResolver keyInfoCredentialResolver = SecurityTestHelper.buildBasicInlineKeyInfoResolver();			
        ExplicitKeySignatureTrustEngine signatureTrustEngine = new ExplicitKeySignatureTrustEngine(staticCredentialResolver, keyInfoCredentialResolver);

		SAML2HTTPRedirectDeflateSignatureRule httpRedirectDeflateSignatureRule = new SAML2HTTPRedirectDeflateSignatureRule(signatureTrustEngine);			
		httpRedirectDeflateSignatureRule.evaluate(messageContext);
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
			
			if (certificateFileName != null) {
				System.err.println("Loading certificate: " + certificateFileName);
				loadCertificate();
			}

			BufferedReader bufferedReader = new BufferedReader(new FileReader(fileName));			
			String url = bufferedReader.readLine();
			System.err.println("URL: " + url);
			
			System.err.println("Initializing OpenSAML");
			DefaultBootstrap.bootstrap();
			
			BasicSAMLMessageContext<SignableSAMLObject, SignableSAMLObject, NameID> messageContext = createMessageContext(createHttpServletRequest(url));
			
			if (x509Certificate != null) {
				if (isSigned(url)) {
					System.err.println("Verifying SAML signature");
					verifySignature(messageContext);
					System.err.println("Signature verified: " + messageContext.isInboundSAMLMessageAuthenticated());
				} else {
					System.err.println("The SAML is not signed");
				}
			}
			
			System.err.println("Decoding SAML message");
			HTTPRedirectDeflateDecoder httpRedirectDeflateDecoder = new HTTPRedirectDeflateDecoder();
			httpRedirectDeflateDecoder.decode(messageContext);			

			System.err.println("Relay state: " + messageContext.getRelayState());
			
			XMLObject inboundMessage = messageContext.getInboundMessage();
			Marshaller marshaller = Configuration.getMarshallerFactory().getMarshaller(inboundMessage);
			Element samlElement = marshaller.marshall(inboundMessage);
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
