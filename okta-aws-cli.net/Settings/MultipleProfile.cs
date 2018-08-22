using System;
using System.IO;
using System.Linq;

using IniParser.Model;

using okta_aws_cli.net.Models;

namespace okta_aws_cli.net.Settings
{
    public class MultipleProfile : Settings
    {
        const string SOURCE_PROFILE = "source_profile";
        const string PROFILE_EXPIRY = "profile_expiry";
        const string OKTA_SESSION = "okta_roleArn";

        /**
         * Create a Profiles object from a given {@link Reader}. The data given by this {@link Reader} should
         * be INI-formatted.
         *
         * @param reader The settings we want to work with. N.B.: The reader is consumed by the constructor.
         * @throws IOException Thrown when we cannot read or load from the given {@param reader}.
         */
        public MultipleProfile(StreamReader reader)
            : base(reader)
        {
        }

        public Profile GetProfile(string oktaProfile, string profileIni)
        {
            IniData ini = this.iniDataParser.ReadFile(profileIni);

            if (ini.Sections.Any(s => s.SectionName.ToLower() == oktaProfile.ToLower()))
            {
                var expiry = GetExpiry(oktaProfile, ini);
                var roleArn = GetRoleArn(oktaProfile, ini);

                return new Profile(expiry, roleArn);
            }

            return null;
        }

        public void DeleteProfile(string profileStore, string oktaProfile)
        {
            using (var inputStream = new FileStream(profileStore, FileMode.Open))
            {
                using (var streamReader = new StreamReader(inputStream))
                {
                    IniData ini = this.iniDataParser.ReadData(streamReader);
                    ini.Sections.RemoveSection(oktaProfile);

                    this.iniDataParser.WriteFile(profileStore, ini);
                }
            }
        }

        private DateTime GetExpiry(string oktaProfile, IniData ini)
        {
            var profileSection = ini[oktaProfile];
            var profileExpiry = profileSection["profile_expiry"];

            DateTime expiry;
            DateTime.TryParse(profileExpiry, out expiry);

            return expiry;
        }

        private string GetRoleArn(string oktaprofile, IniData ini)
        {
            var profileSection = ini[oktaprofile];
            var roleArn = profileSection["okta_roleArn"];

            return roleArn;
        }

        /**
         * Add or update a profile to an Okta Profile file based on {@code name}. This will be linked to a okta profile
         * of the same {@code name}, which should already be present in the profile expiry file.
         *
         * @param name         The name of the profile.
         * @param expiry       expiry time of the profile session.
         * @param okta_session the expiry time of the okta session
         */
        public void AddOrUpdateProfile(string name, string okta_session, DateTime expiry)
        {
            if (!settings.Sections.ContainsSection(name))
            {
                settings.Sections.AddSection(name);
            }

            var awsProfile = settings.Sections.GetSectionData(name);

            this.WriteSessionProfile(awsProfile, name, okta_session, expiry);
        }

        private void WriteSessionProfile(SectionData awsProfile, string name, string okta_session, DateTime expiry)
        {
            awsProfile.Keys.AddKey(SOURCE_PROFILE, name);
            awsProfile.Keys.AddKey(OKTA_SESSION, okta_session);
            awsProfile.Keys.AddKey(PROFILE_EXPIRY, expiry.ToString("o"));
        }
    }
}
