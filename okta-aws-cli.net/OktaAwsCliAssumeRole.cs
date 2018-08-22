using System;
using Amazon.SecurityToken.Model;
using log4net;
using log4net.Core;

using okta_aws_cli.net.Helpers;
using okta_aws_cli.net.Models;
using okta_aws_cli.net.Saml;

namespace okta_aws_cli.net
{
    public class OktaAwsCliAssumeRole
    {
        private static readonly ILogger logger = LogManager.GetLogger(nameof(OktaAwsCliAssumeRole)).Logger;

        private OktaAwsCliEnvironment environment;

        private SessionHelper sessionHelper;
        private ConfigHelper configHelper;
        private RoleHelper roleHelper;
        private ProfileHelper profileHelper;

        private OktaSaml oktaSaml;

        private Session currentSession;
        private Profile currentProfile;

        private OktaAwsCliAssumeRole(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        public static OktaAwsCliAssumeRole WithEnvironment(OktaAwsCliEnvironment environment)
        {
            return new OktaAwsCliAssumeRole(environment);
        }

        private void Init()
        {
            sessionHelper = new SessionHelper(environment);
            configHelper = new ConfigHelper(environment);
            roleHelper = new RoleHelper(environment);
            profileHelper = new ProfileHelper(environment);

            oktaSaml = new OktaSaml(environment);

            currentSession = sessionHelper.GetCurrentSession();

            if (string.IsNullOrWhiteSpace(environment.oktaProfile))
            {
                if (currentSession.IsPresent())
                {
                    environment.oktaProfile = currentSession.ProfileName;
                }
            }

            currentProfile = sessionHelper.GetFromMultipleProfiles();
        }

        public string Run(DateTime startInstant)
        {
            this.Init();

            environment.awsRoleToAssume = currentProfile.RoleArn;

            if (currentSession.IsPresent() && sessionHelper.SessionIsActive(startInstant, currentSession) &&
                    string.IsNullOrWhiteSpace(environment.oktaProfile))
            {
                return currentSession.ProfileName;
            }

            var samlResponse = oktaSaml.GetSamlResponse();
            AssumeRoleWithSAMLRequest assumeRequest = roleHelper.ChooseAwsRoleToAssume(samlResponse);
            DateTime sessionExpiry = startInstant.AddSeconds(assumeRequest.DurationSeconds - 30);
            AssumeRoleWithSAMLResult assumeResult = roleHelper.AssumeChosenAwsRole(assumeRequest);
            String profileName = profileHelper.CreateAwsProfile(assumeResult);

            environment.oktaProfile = profileName;
            environment.awsRoleToAssume = assumeRequest.RoleArn;
            configHelper.UpdateConfigFile();
            sessionHelper.AddOrUpdateProfile(sessionExpiry);
            sessionHelper.UpdateCurrentSession(sessionExpiry, profileName);

            return profileName;
        }

        public void LogoutSession()
        {
            this.Init();

            sessionHelper.LogoutCurrentSession();
        }
    }
}
