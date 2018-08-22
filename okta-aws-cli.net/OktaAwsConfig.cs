using System;

using okta_aws_cli.net.Helpers;

namespace okta_aws_cli.net
{
    public class OktaAwsConfig
    {
        private const string CONFIG_FILENAME = "config.properties";

        public static OktaAwsCliEnvironment LoadEnvironment()
        {
            var properties = new Properties(GetConfigFile());

            return new OktaAwsCliEnvironment(
                    GetBrowserAuthFlag(GetEnvOrConfig(properties, "OKTA_BROWSER_AUTH")),
                    GetEnvOrConfig(properties, "OKTA_ORG"),
                    GetEnvOrConfig(properties, "OKTA_USERNAME"),
                    GetEnvOrConfig(properties, "OKTA_PASSWORD"),
                    GetEnvOrConfig(properties, "OKTA_PROFILE"),
                    GetEnvOrConfig(properties, "OKTA_AWS_APP_URL"),
                    GetEnvOrConfig(properties, "OKTA_AWS_ROLE_TO_ASSUME"),
                    GetStsDurationOrDefault(GetEnvOrConfig(properties, "OKTA_STS_DURATION"))
            );
        }

        private static string GetConfigFile()
        {
            var oktaDirectoryPath = FileHelper.GetOktaDirectory();
            var configInOktaDir = oktaDirectoryPath + CONFIG_FILENAME;

            return configInOktaDir;
        }

        private static bool GetBrowserAuthFlag(string browserAuthString)
        {
            bool browserAuth;
            bool.TryParse(browserAuthString, out browserAuth);

            return browserAuth;
        }

        private static string GetEnvOrConfig(Properties properties, string propertyName)
        {
            string envValue = Environment.GetEnvironmentVariable(propertyName, EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }

            return properties.GetProperty(propertyName);
        }

        private static int GetStsDurationOrDefault(string stsDuration)
        {
            int duration;
            int.TryParse(stsDuration, out duration);

            return (duration > 0)
                ? duration
                : 3600;
        }
    }
}
