using System;
using System.Collections.Generic;

using okta_aws_cli.net.Saml.Models;

namespace okta_aws_cli.net.Saml
{
    public sealed class AssertionUtils
    {
        public static List<string> GetAttributeValues(SamlAssertion assertion, string attributeName)
        {
            throw new NotImplementedException();
            /*return assertion.getAttributeStatements()
                    .stream()
                    .flatMap(x->x.getAttributes().stream())
                    .filter(x->attributeName.equals(x.getName()))
                    .flatMap(x->x.getAttributeValues().stream())
                    .map(AssertionUtils::getAttributeValue)
                    .filter(x->x != null && !x.isEmpty())
                    .collect(Collectors.toList());*/
        }
    }
}
