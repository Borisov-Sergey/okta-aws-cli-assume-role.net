using System;
using System.Collections.Generic;

using Supremes.Nodes;

using okta_aws_cli.net.Models;

namespace okta_aws_cli.net.Saml
{
    public static class AwsSamlSigninParser
    {
        public static List<AccountOption> ParseAccountOptions(Document document)
        {
            var accountOptions = new List<AccountOption>();
            Elements accountElements = document.Select("fieldset > div.saml-account");

            foreach (Element accountElement in accountElements)
            {
                Elements accountNameElements = accountElement.Select("div.saml-account-name");
                Element accountNameElement = accountNameElements[0];
                String accountName = accountNameElement.Text;
                Elements roleOptionElements = accountElement.Select("label.saml-role-description");

                List<RoleOption> roleOptions = new List<RoleOption>();
                foreach (Element roleOptionElement in roleOptionElements)
                {
                    String roleName = roleOptionElement.Text;
                    String roleArn = roleOptionElement.Attr("for");
                    roleOptions.Add(new RoleOption(roleName, roleArn));
                }

                accountOptions.Add(new AccountOption(accountName, roleOptions));
            }
            return accountOptions;
        }
    }
}
