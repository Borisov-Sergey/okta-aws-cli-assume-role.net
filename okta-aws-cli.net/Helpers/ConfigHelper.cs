using System.IO;

using okta_aws_cli.net.Settings;

namespace okta_aws_cli.net.Helpers
{
    public sealed class ConfigHelper
    {
        private OktaAwsCliEnvironment environment;

        public ConfigHelper(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        /**
         * Gets a reader for the config file. If the file doesn't exist, it creates it
         *
         * @return A {@link Reader} for the config file
         * @throws IOException
         */
        public StreamReader GetConfigReader()
        {
            return FileHelper.GetReader(FileHelper.GetAwsDirectory(), "config");
        }

        /**
         * Gets a FileWriter for the config file
         *
         * @return A {@link FileWriter} for the config file
         * @throws IOException
         */
        public StreamWriter GetConfigWriter()
        {
            return FileHelper.GetWriter(FileHelper.GetAwsDirectory(), "config");
        }

        /**
         * Updates the configuration file
         *
         * @throws IOException
         */
        public void UpdateConfigFile()
        {
            using (var reader = GetConfigReader())
            {
                // Create the configuration object with the data from the config file
                var configuration = new Configuration(reader);

                // Write the given profile data
                configuration.AddOrUpdateProfile(environment.oktaProfile, environment.awsRoleToAssume);

                // Write the updated profile
                using (var fileWriter = GetConfigWriter())
                {
                    configuration.Save(fileWriter);
                }
            }
        }
    }
}
