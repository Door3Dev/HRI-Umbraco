@echo off

rem Generate an XML signature over the SAML assertion contained within the specified file.

setlocal

set CLASSPATH=bin\SAMLExamples.jar;lib\opensaml-2.2.0.jar;lib\bcprov-ext-jdk15-1.40.jar;lib\commons-codec-1.3.jar;lib\commons-collections-3.1.jar;lib\commons-httpclient-3.1.jar;lib\commons-lang-2.1.jar;lib\jargs-1.0;lib\jcl-over-slf4j-1.5.2.jar;lib\joda-time-1.5.2.jar;lib\log4j-over-slf4j-1.5.2.jar;lib\not-yet-commons-ssl-0.3.9.jar;lib\openws-1.2.0.jar;lib\slf4j-api-1.5.2.jar;lib\slf4j-nop-1.5.0.jar;lib\velocity-1.5.jar;lib\xmlsec-1.4.2.jar;lib\xmltooling-1.1.0.jar

java test.SignSAML %*

endlocal
