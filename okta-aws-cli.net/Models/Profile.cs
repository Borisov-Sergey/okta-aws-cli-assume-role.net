using System;

namespace okta_aws_cli.net.Models
{
    public class Profile
    {
        public DateTime Expiry { get; private set; }
        public string RoleArn { get; private set; }

        public Profile(DateTime expiry, string roleArn)
        {
            this.Expiry = expiry;
            this.RoleArn = roleArn;
        }
    }
}
