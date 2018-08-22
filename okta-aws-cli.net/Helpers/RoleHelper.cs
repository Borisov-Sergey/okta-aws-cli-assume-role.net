using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using okta_aws_cli.net.Models;
using okta_aws_cli.net.Saml;

namespace okta_aws_cli.net.Helpers
{
    public sealed class RoleHelper
    {
        private OktaAwsCliEnvironment environment;

        public RoleHelper(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        public AssumeRoleWithSAMLResult AssumeChosenAwsRole(AssumeRoleWithSAMLRequest assumeRequest)
        {
            /*BasicAWSCredentials nullCredentials = new BasicAWSCredentials("", "");
            AWSCredentialsProvider nullCredentialsProvider = new AWSStaticCredentialsProvider(nullCredentials);
            IAmazonSecurityTokenService sts = AmazonSecurityTokenServiceClientBuilder
                    .standard()
                    .withRegion(Regions.US_EAST_1)
                    .withCredentials(nullCredentialsProvider)
                    .build();
            
            return sts.assumeRoleWithSAML(assumeRequest);*/

            var config = new AmazonSecurityTokenServiceConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.EUWest1
            };

            var stsClient = new AmazonSecurityTokenServiceClient(config);

            var response = stsClient.AssumeRoleWithSAML(assumeRequest);
            return response;
        }

        public AssumeRoleWithSAMLRequest ChooseAwsRoleToAssume(string samlAssertion)
        {
            Dictionary<string, string> roleIdpPairs = AwsSamlRoleUtils.GetRoles(samlAssertion);
            List<string> roleArns = new List<string>();

            string principalArn;
            string roleArn;

            if (roleIdpPairs.ContainsKey(environment.awsRoleToAssume))
            {
                principalArn = roleIdpPairs[environment.awsRoleToAssume];
                roleArn = environment.awsRoleToAssume;
            }
            else if (roleIdpPairs.Count > 1)
            {
                List<AccountOption> accountOptions = GetAvailableRoles(samlAssertion);

                Console.WriteLine("\nPlease choose the role you would like to assume: ");
                //Gather list of applicable AWS roles
                int i = 0;
                int j = -1;

                foreach (AccountOption accountOption in accountOptions)
                {
                    Console.WriteLine(accountOption.accountName);

                    foreach (RoleOption roleOption in accountOption.roleOptions)
                    {
                        roleArns.Add(roleOption.roleArn);
                        Console.WriteLine("\t[ " + (i + 1) + " ]: " + roleOption.roleName);

                        if (roleOption.roleArn.Equals(environment.awsRoleToAssume))
                        {
                            j = i;
                        }

                        i++;
                    }
                }

                if ((environment.awsRoleToAssume != null && !string.IsNullOrWhiteSpace(environment.awsRoleToAssume)) && j == -1)
                {
                    Console.WriteLine("No match for role " + environment.awsRoleToAssume);
                }

                // Default to no selection
                int selection;

                // If config.properties has matching role, use it and don't prompt user to select
                if (j >= 0)
                {
                    selection = j;
                    Console.WriteLine("Selected option " + (j + 1) + " based on OKTA_AWS_ROLE_TO_ASSUME value");
                }
                else
                {
                    //Prompt user for role selection
                    selection = MenuHelper.PromptForMenuSelection(roleArns.Count);
                }

                roleArn = roleArns[selection];
                principalArn = roleIdpPairs[roleArn];
            }
            else
            {
                var role = roleIdpPairs.First();
                Console.WriteLine("Auto select role as only one is available : " + role.Key);

                roleArn = role.Key;
                principalArn = role.Value;
            }

            var request = new AssumeRoleWithSAMLRequest
            {
                PrincipalArn = principalArn,
                RoleArn = roleArn,
                SAMLAssertion = samlAssertion,
                DurationSeconds = environment.stsDuration
            };

            return request;
        }

        private List<AccountOption> GetAvailableRoles(string samlResponse)
        {
            var document = AwsSamlRoleUtils.GetSigninPageDocument(samlResponse);
            return AwsSamlSigninParser.ParseAccountOptions(document);
        }
    }
}
