using System;

namespace okta_aws_cli.net
{
    public class OktaAwsCliEnvironment
    {
        public readonly bool browserAuth;
        public readonly string oktaOrg;
        public readonly string oktaUsername;
        public readonly string oktaPassword;
        public readonly string oktaAwsAppUrl;

        public string oktaProfile;
        public string awsRoleToAssume;
        public int stsDuration;

        public Uri OktaAwsAppUri => new Uri(oktaAwsAppUrl);

        public OktaAwsCliEnvironment(bool browserAuth, string oktaOrg, string oktaUsername, string oktaPassword, string oktaProfile,
                                     string oktaAwsAppUrl, string awsRoleToAssume, int stsDuration)
        {
            this.browserAuth = browserAuth;
            this.oktaOrg = oktaOrg;
            this.oktaUsername = oktaUsername;
            this.oktaPassword = oktaPassword;
            this.oktaProfile = oktaProfile;
            this.oktaAwsAppUrl = oktaAwsAppUrl;
            this.awsRoleToAssume = awsRoleToAssume;
            this.stsDuration = stsDuration;
        }
    }
}
