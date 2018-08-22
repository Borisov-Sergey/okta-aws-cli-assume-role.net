using System;
using System.IO;

using okta_aws_cli.net.Models;
using okta_aws_cli.net.Settings;

namespace okta_aws_cli.net.Helpers
{
    public sealed class SessionHelper
    {
        private const string OKTA_AWS_CLI_EXPIRY_PROPERTY = "OKTA_AWS_CLI_EXPIRY";
        private const string OKTA_AWS_CLI_PROFILE_PROPERTY = "OKTA_AWS_CLI_PROFILE";

        private OktaAwsCliEnvironment environment;

        public SessionHelper(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        /**
         * Gets the current session file's path
         *
         * @return A {@link Path} for the current session file
         * @throws IOException
         */
        private string GetSessionPath()
        {
            return FileHelper.ResolveFilePath(FileHelper.GetOktaDirectory(), ".current-session");
        }

        /**
         * Gets the current session, if it exists
         *
         * @return The current {@link Session}
         * @throws IOException
         */
        public Session GetCurrentSession()
        {
            if (File.Exists(GetSessionPath()))
            {
                var properties = new Properties(GetSessionPath());
                var expiry = properties.GetProperty(OKTA_AWS_CLI_EXPIRY_PROPERTY);
                var profileName = properties.GetProperty(OKTA_AWS_CLI_PROFILE_PROPERTY);

                DateTime expiryInstant;
                DateTime.TryParse(expiry, out expiryInstant);

                return new Session(profileName, expiryInstant);
            }

            return null;
        }

        /**
         * Deletes the current session, if it exists
         *
         * @throws IOException
         */
        public void LogoutCurrentSession()
        {
            if (!string.IsNullOrWhiteSpace(environment.oktaProfile))
            {
                LogoutMultipleAccounts(environment.oktaProfile);
            }

            var sessionPath = this.GetSessionPath();
            if (File.Exists(sessionPath))
            {
                File.Delete(sessionPath);
            }
        }

        public void UpdateCurrentSession(DateTime expiryInstant, string profileName)
        {
            Properties properties = new Properties(GetSessionPath());
            properties.SetProperty(OKTA_AWS_CLI_PROFILE_PROPERTY, profileName);
            properties.SetProperty(OKTA_AWS_CLI_EXPIRY_PROPERTY, expiryInstant.ToString());
            properties.Save();
        }

        public Profile GetFromMultipleProfiles()
        {
            return getMultipleProfile().GetProfile(environment.oktaProfile, GetMultipleProfilesPath());
        }

        public void AddOrUpdateProfile(DateTime start)
        {
            MultipleProfile multipleProfile = getMultipleProfile();
            multipleProfile.AddOrUpdateProfile(environment.oktaProfile, environment.awsRoleToAssume, start);

            using (var fileWriter = FileHelper.GetWriter(FileHelper.GetOktaDirectory(), "profiles"))
            {
                multipleProfile.Save(fileWriter);
            }
        }

        public bool SessionIsActive(DateTime startInstant, Session session)
        {
            return startInstant < session.Expiry;
        }

        private void LogoutMultipleAccounts(string profileName)
        {
            CookieHelper.ClearCookies();

            getMultipleProfile().DeleteProfile(GetMultipleProfilesPath(), profileName);
        }

        private string GetMultipleProfilesPath()
        {
            return FileHelper.GetFilePath(FileHelper.GetOktaDirectory(), "profiles");
        }

        private MultipleProfile getMultipleProfile()
        {
            var reader = FileHelper.GetReader(FileHelper.GetOktaDirectory(), "profiles");

            return new MultipleProfile(reader);
        }
    }
}
