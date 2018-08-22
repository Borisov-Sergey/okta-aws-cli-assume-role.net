using System;
using System.IO;
using System.Xml;

using okta_aws_cli.net.Saml.Models;

namespace okta_aws_cli.net.Saml
{
    public sealed class SamlResponseUtils
    {
        /*static SamlResponseUtils()
        {
            try
            {
                new JavaCryptoValidationInitializer().init();
                InitializationService.initialize();
            }
            catch (InitializationException e)
            {
                throw new RuntimeException(e);
            }
        
        }*/

        public static SamlAssertion GetAssertion(string samlResponse)
        {
            SamlResponse response = DecodeSamlResponse(samlResponse);
            return GetAssertion(response);
        }

        private static SamlResponse DecodeSamlResponse(string samlResponse)
        {
            var base64DecodedResponse = Convert.FromBase64String(samlResponse);
            var inputStream = new MemoryStream(base64DecodedResponse);

            var document = new XmlDocument();
            document.Load(inputStream);

            throw new NotImplementedException();//try to use XmlSerializer.
            /*var element = document.FirstChild;
            XMLObject responseXmlObj = new ResponseUnmarshaller().unmarshall(element);

            return (SamlResponse)responseXmlObj;*/
        }

        private static SamlAssertion GetAssertion(SamlResponse response)
        {
            if (response.Assertions.Count == 0)
                throw new SystemException("No assertions in SAML response");

            else if (response.Assertions.Count > 1)
                throw new SystemException("More than one assertion in SAML response");

            else
                return response.Assertions[0];
        }
    }
}
