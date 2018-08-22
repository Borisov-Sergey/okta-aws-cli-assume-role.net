using System;
using System.IO;
using System.Net;
using System.Text;

using log4net;

using Newtonsoft.Json.Linq;

using okta_aws_cli.net.Models;

namespace okta_aws_cli.net.Authentication
{
    public sealed class OktaAuthentication
    {
        private static readonly ILog logger = LogManager.GetLogger(nameof(OktaAuthentication));

        private OktaAwsCliEnvironment environment;

        public OktaAuthentication(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        /**
         * Performs primary and secondary (2FA) authentication, then returns a session token
         *
         * @return The session token
         * @throws IOException
         */
        public string GetOktaSessionToken()
        {
            var response = GetPrimaryAuthResponse(environment.oktaOrg);

            var primaryAuthResult = JObject.Parse(response);
            if (primaryAuthResult.GetValue("status").Equals("MFA_REQUIRED"))
            {
                return OktaMFA.PromptForFactor(primaryAuthResult);
            }
            else
            {
                return primaryAuthResult.GetValue("sessionToken").Value<string>();
            }
        }

        /**
         * Performs primary authentication and parses the response.
         *
         * @param oktaOrg The org to authenticate against
         * @return The response of the authentication
         * @throws IOException
         */
        private string GetPrimaryAuthResponse(string oktaOrg)
        {
            while (true)
            {
                AuthResult response = PrimaryAuthentication(GetUsername(), GetPassword(), oktaOrg);

                PrimaryAuthFailureHandler(response.StatusCode, oktaOrg);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response.ResponseContent;
                }
            }
        }

        /**
         * Perform primary authentication against Okta
         *
         * @param username The username of the user
         * @param password The password of the user
         * @param oktaOrg  The org to perform auth against
         * @return The authentication result
         * @throws IOException
         */
        private AuthResult PrimaryAuthentication(string username, string password, string oktaOrg)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://" + oktaOrg + "/api/v1/authn");
            httpRequest.Accept = "application/json";
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            httpRequest.Headers.Add("Cache-Control", "no-cache");

            var jsonObject = new JObject();
            jsonObject.Add("username", username);
            jsonObject.Add("password", password);

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream(), Encoding.UTF8))
            {
                streamWriter.Write(jsonObject.ToString());
                streamWriter.Flush();
            }

            using (var authResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                using (var streamReader = new StreamReader(authResponse.GetResponseStream()))
                {
                    return new AuthResult(authResponse.StatusCode, streamReader.ReadToEnd());
                }
            }
        }

        /**
         * Handles failures during the primary authentication flow
         *
         * @param responseStatus The status of the response
         * @param oktaOrg        The org against which authentication was performed
         */
        private void PrimaryAuthFailureHandler(HttpStatusCode responseStatus, string oktaOrg)
        {
            if (responseStatus == HttpStatusCode.BadRequest || responseStatus == HttpStatusCode.Unauthorized)
            {
                logger.Error("Invalid username or password.");
            }
            else if (responseStatus == HttpStatusCode.InternalServerError)
            {
                logger.Error("\nUnable to establish connection with: " + oktaOrg +
                        " \nPlease verify that your Okta org url is correct and try again");
            }
            else if (responseStatus != HttpStatusCode.OK)
            {
                throw new Exception("Failed : HTTP error code : " + responseStatus);
            }
        }

        private string GetUsername()
        {
            if (string.IsNullOrWhiteSpace(environment.oktaUsername))
            {
                Console.Write("Username: ");
                return Console.ReadLine();
                //return new Scanner(System.in).next();
            }
            else
            {
                Console.WriteLine("Username: " + environment.oktaUsername);
                return environment.oktaUsername;
            }
        }

        private string GetPassword()
        {
            if (string.IsNullOrWhiteSpace(environment.oktaPassword))
            {
                return PromptForPassword();
            }
            else
            {
                return environment.oktaPassword;
            }
        }

        private string PromptForPassword()
        {
            Console.Write("Password: ");
            return Console.ReadLine();
            /*if (System.console() == null)
            { // hack to be able to debug in an IDE
                System.out.print("Password: ");
                return new Scanner(System.in).next();
            }
            else
            {
                return new string(System.console().readPassword("Password: "));
            }*/
        }
    }
}
