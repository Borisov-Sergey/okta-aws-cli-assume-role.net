using Amazon.SecurityToken.Model;

namespace okta_aws_cli.net.Helpers
{
    public sealed class ProfileHelper
    {
        private OktaAwsCliEnvironment environment;

        public ProfileHelper(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        public string CreateAwsProfile(AssumeRoleWithSAMLResult assumeResult)
        {
            var creds = assumeResult.Credentials;
            var credentialsProfileName = GetProfileName(assumeResult, environment.oktaProfile);

            CredentialsHelper.UpdateCredentialsFile(credentialsProfileName, creds.AccessKeyId, creds.SecretAccessKey, creds.SessionToken);

            return credentialsProfileName;
        }

        private string GetProfileName(AssumeRoleWithSAMLResult assumeResult, string oktaProfile)
        {
            // TODO: profile name should have a constant name to be recognizable by other apps
            string credentialsProfileName;

            if (!string.IsNullOrWhiteSpace(oktaProfile))
            {
                credentialsProfileName = oktaProfile;
            }
            else
            {
                credentialsProfileName = assumeResult.AssumedRoleUser.Arn;

                if (credentialsProfileName.StartsWith("arn:aws:sts::"))
                {
                    credentialsProfileName = credentialsProfileName.Substring(13);
                }

                if (credentialsProfileName.Contains(":assumed-role"))
                {
                    credentialsProfileName = credentialsProfileName.Replace(":assumed-role", "");
                }
            }

            return credentialsProfileName;
        }
    }
}
