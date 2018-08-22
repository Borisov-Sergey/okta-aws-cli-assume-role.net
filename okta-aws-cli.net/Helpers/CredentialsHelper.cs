using System.IO;

using okta_aws_cli.net.Settings;

namespace okta_aws_cli.net.Helpers
{
    public sealed class CredentialsHelper
    {
        public static StreamReader GetCredsReader()
        {
            return FileHelper.GetReader(FileHelper.GetAwsDirectory(), "credentials");
        }

        /**
         * Gets a FileWriter for the credentials file
         *
         * @return A {@link FileWriter} for the credentials file
         * @throws IOException
         */
        public static StreamWriter GetCredsWriter()
        {
            return FileHelper.GetWriter(FileHelper.GetAwsDirectory(), "credentials");
        }

        /**
         * Updates the credentials file
         *
         * @param profileName     The profile to use
         * @param awsAccessKey    The access key to use
         * @param awsSecretKey    The secret key to use
         * @param awsSessionToken The session token to use
         * @throws IOException
         */
        public static void UpdateCredentialsFile(string profileName, string awsAccessKey, string awsSecretKey, string awsSessionToken)
        {
            using (var reader = GetCredsReader())
            {
                // Create the credentials object with the data read from the credentials file
                var credentials = new Credentials(reader);

                // Write the given profile data
                credentials.AddOrUpdateProfile(profileName, awsAccessKey, awsSecretKey, awsSessionToken);

                // Write the updated profile
                using (var fileWriter = GetCredsWriter())
                {
                    credentials.Save(fileWriter);
                }
            }
        }
    }
}
