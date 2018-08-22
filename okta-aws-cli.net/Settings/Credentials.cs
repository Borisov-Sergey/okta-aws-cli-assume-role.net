using System.IO;

using IniParser.Model;

namespace okta_aws_cli.net.Settings
{
    public sealed class Credentials : Settings
    {
        static readonly string ACCES_KEY_ID = "aws_access_key_id";
        static readonly string SECRET_ACCESS_KEY = "aws_secret_access_key";
        static readonly string SESSION_TOKEN = "aws_session_token";

        /**
         * Create a Credentials object from a given {@link Reader}. The data given by this {@link Reader} should
         * be INI-formatted.
         *
         * @param reader The settings we want to work with. N.B.: The reader is consumed by the constructor.
         * @throws IOException Thrown when we cannot read or load from the given {@param reader}.
         */
        public Credentials(StreamReader reader)
                : base(reader)
        {
        }

        /**
         * Add or update a profile to an AWS credentials file based on {@code name}.
         *
         * @param name            The name of the profile.
         * @param awsAccessKey    The access key to use for the profile.
         * @param awsSecretKey    The secret key to use for the profile.
         * @param awsSessionToken The session token to use for the profile.
         */
        public void AddOrUpdateProfile(string name, string awsAccessKey, string awsSecretKey, string awsSessionToken)
        {
            name = name + "_source";

            if (!settings.Sections.ContainsSection(name))
            {
                settings.Sections.AddSection(name);
            }

            var awsProfile = settings.Sections.GetSectionData(name);

            WriteCredentialsProfile(awsProfile, awsAccessKey, awsSecretKey, awsSessionToken);
        }

        /**
         * Create a new profile in this credentials object.
         *
         * @param awsProfile      A reference to the profile in the credentials.
         * @param awsAccessKey    The AWS access key to use in the profile.
         * @param awsSecretKey    The AWS secret access key to use in the profile.
         * @param awsSessionToken The AWS session token to use in the profile.
         */
        private void WriteCredentialsProfile(SectionData awsProfile, string awsAccessKey, string awsSecretKey, string awsSessionToken)
        {
            awsProfile.Keys.AddKey(ACCES_KEY_ID, awsAccessKey);
            awsProfile.Keys.AddKey(SECRET_ACCESS_KEY, awsSecretKey);
            awsProfile.Keys.AddKey(SESSION_TOKEN, awsSessionToken);
        }
    }
}
