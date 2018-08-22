using System.Collections.Generic;

namespace okta_aws_cli.net.Saml.Models
{
    public sealed class SamlResponse
    {
        private const string DEFAULT_ELEMENT_LOCAL_NAME = "Response";

        public List<SamlAssertion> Assertions { get; }
    }
}