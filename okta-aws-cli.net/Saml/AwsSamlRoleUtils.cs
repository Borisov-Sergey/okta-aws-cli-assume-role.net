using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

using Supremes;
using Supremes.Nodes;

namespace okta_aws_cli.net.Saml
{
    public sealed class AwsSamlRoleUtils
    {
        private const string AWS_ROLE_SAML_ATTRIBUTE = "https://aws.amazon.com/SAML/Attributes/Role";

        public static Dictionary<string, string> GetRoles(string samlResponse)
        {
            var roles = new Dictionary<string, string>();

            foreach (string roleIdpPair in GetRoleIdpPairs(samlResponse))
            {
                string[] parts = roleIdpPair.Split(',');

                string principalArn = parts[0];
                string roleArn = parts[1];

                roles.Add(roleArn, principalArn);
            }

            return roles;
        }

        private static List<string> GetRoleIdpPairs(string samlResponse)
        {
            var assertion = SamlResponseUtils.GetAssertion(samlResponse);
            return AssertionUtils.GetAttributeValues(assertion, AWS_ROLE_SAML_ATTRIBUTE);
        }

        public static Document GetSigninPageDocument(string samlResponse)
        {
            const string signInUrl = "https://signin.aws.amazon.com/saml";

            var samlForm = new NameValueCollection();
            samlForm.Add("SAMLResponse", samlResponse);
            samlForm.Add("RelayState", "");

            var content = new StringContent(JsonConvert.SerializeObject(samlForm), Encoding.UTF8, "application/x-www-form-urlencoded");

            var httpClient = new HttpClient();
            var samlSignInResponse = httpClient.PostAsync(signInUrl, content).Result;
            if (samlSignInResponse.StatusCode == HttpStatusCode.OK)
            {
                return Dcsoup.Parse(samlSignInResponse.Content.ReadAsStreamAsync().Result, Encoding.UTF8.EncodingName, signInUrl);
            }

            throw new System.Exception("Cannot Authenticate");
        }
    }
}
