using System.IO;
using System.Linq;

using IniParser.Model;

namespace okta_aws_cli.net.Settings
{
    public class Configuration : Settings
    {
        const string ROLE_ARN = "role_arn";
        const string SOURCE_PROFILE = "source_profile";
        const string REGION = "region";
        const string PROFILE_PREFIX = "profile ";

        private const string REGION_DEFAULT = "eu-west-1";

        /**
         * Create a Configuration object from a given {@link Reader}. The data given by this {@link Reader} should
         * be INI-formatted.
         *
         * @param reader The settings we want to work with. N.B.: The reader is consumed by the constructor.
         * @throws IOException Thrown when we cannot read or load from the given {@code reader}.
         */
        public Configuration(StreamReader reader)
            : base(reader)
        {
        }

        /**
         * Add or update a profile to an AWS config file based on {@code name}. This will be linked to a credential profile
         * of the same {@code name}, which should already be present in the credentials file.
         * The region for this new profile will be {@link Configuration#REGION_DEFAULT}.
         *
         * @param name         The name of the profile.
         * @param roleToAssume The ARN of the role to assume in this profile.
         */
        public void AddOrUpdateProfile(string name, string roleToAssume)
        {
            // profileName is the string used for the section in the AWS config file.
            // This should be prefixed with "profile ".
            string profileName = PROFILE_PREFIX + name;

            // Determine whether this is a new AWS configuration file. If it is, we'll set the default
            // profile to this profile.
            if (!settings.Sections.Any())
            {
                var defaultAwsProfile = new SectionData(DEFAULTPROFILENAME);
                settings.Sections.Add(defaultAwsProfile);

                writeConfigurationProfile(defaultAwsProfile, name, roleToAssume);
            }

            // Write the new profile data
            var awsProfile = settings.Sections.GetSectionData(profileName);
            if (awsProfile == null)
            {
                awsProfile = new SectionData(profileName);
                settings.Sections.Add(awsProfile);
            }

            writeConfigurationProfile(awsProfile, name, roleToAssume);
        }

        private void writeConfigurationProfile(SectionData awsProfile, string name, string roleToAssume)
        {
            awsProfile.Keys.AddKey(ROLE_ARN, roleToAssume);
            awsProfile.Keys.AddKey(SOURCE_PROFILE, name + "_source");

            if (!awsProfile.Keys.ContainsKey(REGION))
            {
                awsProfile.Keys.AddKey(REGION, REGION_DEFAULT);
            }
        }
    }
}
