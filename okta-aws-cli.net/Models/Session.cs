using System;

namespace okta_aws_cli.net.Models
{
    public class Session
    {
        public string ProfileName {get; private set; }
        public DateTime Expiry { get; private set; }

        private bool ProfileNameSpecified => !string.IsNullOrWhiteSpace(this.ProfileName);
        private bool ExpirySpecified => this.Expiry != DateTime.MaxValue && this.Expiry != DateTime.MinValue;

        public Session(String profileName, DateTime expiry)
        {
            this.ProfileName = profileName;
            this.Expiry = expiry;
        }

        public bool IsPresent()
        {
            return this.ProfileNameSpecified || this.ExpirySpecified;
        }
    }
}
