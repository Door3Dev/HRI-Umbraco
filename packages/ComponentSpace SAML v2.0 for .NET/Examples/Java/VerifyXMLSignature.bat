@echo off

rem Verify an XML signature over the XML contained within the specified file.

setlocal

set CLASSPATH=bin\SAMLExamples.jar

java -Djava.util.logging.config.file=logging.properties test.VerifyXMLSignature %*

endlocal
