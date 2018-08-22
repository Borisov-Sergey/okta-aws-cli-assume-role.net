using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using okta_aws_cli.net.Helpers;

namespace okta_aws_cli.net.Authentication
{
    public sealed class OktaMFA
    {
        /**
         * Prompt the user for 2FA after primary authentication
         *
         * @param primaryAuthResponse The response from primary auth
         * @return The session token
         */
        public static string PromptForFactor(JObject primaryAuthResponse)
        {
            try
            {
                // User selects which factor to use
                JObject factor = SelectFactor(primaryAuthResponse);
                string factorType = factor.GetValue("factorType").Value<string>();
                string stateToken = primaryAuthResponse.GetValue("stateToken").Value<string>();

                // Factor selection handler
                switch (factorType)
                {
                    case ("question"):
                        {
                            // Security Question handler
                            string sessionToken = QuestionFactor(factor, stateToken);
                            return HandleTimeoutsAndChanges(sessionToken, primaryAuthResponse);
                        }
                    case ("sms"):
                        {
                            // SMS handler
                            string sessionToken = SmsFactor(factor, stateToken);
                            return HandleTimeoutsAndChanges(sessionToken, primaryAuthResponse);

                        }
                    case ("token"):
                    case ("token:hardware"):
                    case ("token:software:totp"):
                        {
                            // Token handler
                            string sessionToken = ToTpFactor(factor, stateToken);
                            return HandleTimeoutsAndChanges(sessionToken, primaryAuthResponse);
                        }
                    case ("push"):
                        {
                            // Push handler
                            string sessionToken = PushFactor(factor, stateToken);
                            return HandleTimeoutsAndChanges(sessionToken, primaryAuthResponse);
                        }
                }
            }
            catch (Exception e) when (e is JsonException || e is IOException)
            {
                Console.WriteLine(e.StackTrace);
            }

            return "";
        }

        /**
         * Handles MFA timeouts and factor changes
         *
         * @param sessionToken        The current state of the MFA
         * @param primaryAuthResponse The response from Primary Authentication
         * @return The factor prompt if invalid, session token otherwise
         */
        private static string HandleTimeoutsAndChanges(string sessionToken, JObject primaryAuthResponse)
        {
            if (sessionToken.Equals("change factor"))
            {
                Console.WriteLine("Factor change initiated");
                return PromptForFactor(primaryAuthResponse);
            }
            else if (sessionToken.Equals("timeout"))
            {
                Console.WriteLine("Factor timed out");
                return PromptForFactor(primaryAuthResponse);
            }
            return sessionToken;
        }

        /**
         * Handles selection of a factor from multiple choices
         *
         * @param primaryAuthResponse The response from Primary Authentication
         * @return A {@link JObject} representing the selected factor.
         * @throws JSONException if a network or protocol error occurs
         */
        private static JObject SelectFactor(JObject primaryAuthResponse)
        {
            JArray factors = primaryAuthResponse.GetValue("_embedded").Value<JArray>("factors");
            string factorType;

            List<JObject> supportedFactors = GetUsableFactors(factors);
            if (supportedFactors.Count == 0)
            {
                if (factors.Count > 0)
                {
                    throw new InvalidOperationException("None of your factors are supported.");
                }
                else
                {
                    throw new InvalidOperationException("You have no factors enrolled.");
                }
            }

            if (supportedFactors.Count > 1)
            {
                Console.WriteLine("\nMulti-Factor authentication is required. Please select a factor to use.");
                Console.WriteLine("Factors:");

                for (int i = 0; i < supportedFactors.Count; i++)
                {
                    JObject factor = supportedFactors[i];
                    factorType = factor.Value<string>("factorType");
                    factorType = GetFactorDescription(factorType, factor);

                    Console.WriteLine("[ " + (i + 1) + " ] : " + factorType);
                }
            }

            // Handles user factor selection
            int selection = MenuHelper.PromptForMenuSelection(supportedFactors.Count);
            return supportedFactors[selection];
        }

        private static string GetFactorDescription(string factorType, JObject factor)
        {
            string provider = factor.Value<string>("provider");
            switch (factorType)
            {
                case "push":
                    return "Okta Verify (Push)";

                case "question":
                    return "Security Question";

                case "sms":
                    return "SMS Verification";

                case "call":
                    return "Phone Verification"; // Unsupported

                case "token:software:totp":
                    switch (provider)
                    {
                        case "OKTA":
                            return "Okta Verify (TOTP)";
                        case "GOOGLE":
                            return "Google Authenticator";
                        default:
                            return provider + " " + factorType;
                    }

                case "email":
                    return "Email Verification";  // Unsupported

                case "token":
                    switch (provider)
                    {
                        case "SYMANTEC":
                            return "Symantec VIP";

                        case "RSA":
                            return "RSA SecurID";

                        default:
                            return provider + " " + factorType;
                    }
                case "web":
                    return "Duo Push"; // Unsupported

                case "token:hardware":
                    return "Yubikey";

                default:
                    return provider + " " + factorType;
            }
        }

        /**
         * Selects the supported factors from a list of factors
         *
         * @param factors The list of factors
         * @return A {@link List<JObject>} of supported factors
         */
        private static List<JObject> GetUsableFactors(JArray factors)
        {
            List<JObject> eligibleFactors = new List<JObject>();

            foreach (JObject factor in factors)
            {
                string factorType = factor.Value<string>("factorType");

                if (!new string[] {
                        "web", // Factors that only work on the web cannot be verified via the CLI
                        "call", // Call factor support isn't implemented yet
                        "email"  // Email factor support isn't implemented yet
                }.Any(s => s == factorType.ToLower()))
                {
                    eligibleFactors.Add(factor);
                }
            }

            return eligibleFactors;
        }

        /**
         * Handles the Security Question factor
         *
         * @param factor     A {@link JObject} representing the user's factor
         * @param stateToken The current state token
         * @return The session token
         * @throws IOException if a network or protocol error occurs
         */
        private static string QuestionFactor(JObject factor, string stateToken)
        {
            string question = factor.Value<JObject>("profile").Value<string>("questionText");
            string sessionToken = "";
            string answer = "";

            // Prompt the user for the Security Question Answer
            Console.WriteLine("\nSecurity Question Factor Authentication\nEnter 'change factor' to use a different factor\n");

            while ("".Equals(sessionToken))
            {
                if (!"".Equals(answer))
                {
                    Console.WriteLine("Incorrect answer, please try again");
                }

                Console.WriteLine(question);
                Console.WriteLine("Answer: ");

                answer = Console.ReadLine();

                // Factor change requested
                if (answer.ToLower().Equals("change factor"))
                {
                    return answer;
                }

                // Verify the answer's validity
                sessionToken = VerifyAnswer(answer, factor, stateToken, "question");
            }

            return sessionToken;
        }

        /**
         * Handles the SMS Verification factor
         *
         * @param factor     A {@link JObject} representing the user's factor
         * @param stateToken The current state token
         * @return The session token
         * @throws JSONException
         * @throws IOException
         */
        private static string SmsFactor(JObject factor, string stateToken)
        {
            string answer = "";
            string sessionToken = "";

            Console.WriteLine("\nSMS Factor Authentication \nEnter 'change factor' to use a different factor");

            while ("".Equals(sessionToken))
            {
                if (!"".Equals(answer))
                {
                    Console.WriteLine("Incorrect passcode, please try again or type 'new code' to be sent a new SMS passcode");
                }
                else
                {
                    // Send initial code to the user
                    VerifyAnswer("", factor, stateToken, "sms");
                }

                Console.WriteLine("SMS Code: ");
                answer = Console.ReadLine();

                switch (answer.ToLower())
                {
                    case "new code":
                        // New SMS passcode requested
                        answer = "";
                        Console.WriteLine("New code sent! \n");
                        break;

                    case "change factor":
                        // Factor change requested
                        return answer;
                }

                // Verify the validity of the SMS passcode
                sessionToken = VerifyAnswer(answer, factor, stateToken, "sms");
            }

            return sessionToken;
        }

        /**
         * Handles Token Factor verification
         *
         * @param factor     A {@link JObject} representing the user's factor
         * @param stateToken The current state token
         * @return The session token
         * @throws IOException if a network or protocol error occurs
         */
        private static string ToTpFactor(JObject factor, string stateToken)
        {
            string sessionToken = "";
            string answer = "";

            // Prompt for token
            Console.WriteLine("\n" + factor.Value<string>("provider") + " Token Factor Authentication\nEnter 'change factor' to use a different factor");

            while ("".Equals(sessionToken))
            {
                if (!"".Equals(answer))
                {
                    Console.WriteLine("Invalid token, please try again");
                }

                Console.WriteLine("Token: ");
                answer = Console.ReadLine();

                // Factor change requested
                if (answer.ToLower().Equals("change factor"))
                {
                    return answer;
                }

                // Verify the validity of the token
                sessionToken = VerifyAnswer(answer, factor, stateToken, "token:software:totp");
            }

            return sessionToken;
        }

        /**
         * Handles Push verification
         *
         * @param factor     A {@link JObject} representing the user's factor
         * @param stateToken The current state token
         * @return The session token
         * @throws IOException
         */
        private static string PushFactor(JObject factor, string stateToken)
        {
            string sessionToken = "";
            Console.WriteLine("\nPush Factor Authentication");

            while ("".Equals(sessionToken))
            {
                // Verify if Okta Push has been verified
                sessionToken = VerifyAnswer(null, factor, stateToken, "push");
                Console.WriteLine(sessionToken);

                // Session has timed out
                if (sessionToken.Equals("Timeout"))
                {
                    Console.WriteLine("Session has timed out");
                    return "timeout";
                }
            }

            return sessionToken;
        }

        /**
         * Handles verification for all factor types
         *
         * @param answer     The answer to the factor
         * @param factor     A {@link JObject} representing the factor to be verified
         * @param stateToken The current state token
         * @param factorType The factor type
         * @return The session token
         */
        private static string VerifyAnswer(string answer, JObject factor, string stateToken, string factorType)
        {
            string sessionToken = null;

            JObject profile = new JObject();
            string verifyPoint = factor.Value<JObject>("_links").Value<JObject>("verify").Value<string>("href");

            profile["stateToken"] = stateToken;

            if (answer != null && !"".Equals(answer))
            {
                profile["answer"] = answer;
            }

            StringContent content = new StringContent(profile.ToString(), Encoding.UTF8, "application/json");
            content.Headers.Add("Accept", "application/json");
            content.Headers.Add("Content-Type", "application/json");
            content.Headers.Add("Cache-Control", "no-cache");

            var httpClient = new HttpClient();
            var responseAuthenticate = httpClient.PostAsync(verifyPoint, content).Result;

            string outputAuthenticate = responseAuthenticate.Content.ReadAsStringAsync().Result;
            JObject jsonObjResponse = new JObject(outputAuthenticate);

            // An error has been returned by Okta
            if (jsonObjResponse.ContainsKey("errorCode"))
            {
                string errorSummary = jsonObjResponse.Value<string>("errorSummary");

                Console.WriteLine(errorSummary);
                Console.WriteLine("Please try again");

                if (factorType.Equals("question"))
                {
                    QuestionFactor(factor, stateToken);
                }

                if (factorType.Equals("token:software:totp"))
                {
                    ToTpFactor(factor, stateToken);
                }
            }

            if (jsonObjResponse.ContainsKey("sessionToken"))
            {
                sessionToken = jsonObjResponse.Value<string>("sessionToken");
            }

            // Handle Push
            string pushResult = null;
            if (factorType.Equals("push"))
            {
                if (jsonObjResponse.ContainsKey("_links"))
                {
                    string pollUrl = "";
                    JArray links = jsonObjResponse.Value<JArray>("_links");

                    foreach (JObject link in links)
                    {
                        string linkName = link.Value<string>("name");

                        if (linkName.Equals("poll"))
                        {
                            pollUrl = link.Value<string>("href");
                            break;
                        }
                    }

                    // Wait for push state change
                    while (pushResult == null || pushResult.Equals("WAITING"))
                    {
                        pushResult = null;

                        var responsePush = httpClient.PostAsync(pollUrl, content).Result;

                        string outputTransaction = responsePush.Content.ReadAsStringAsync().Result;
                        JObject jsonTransaction = new JObject(outputTransaction);

                        if (jsonTransaction.ContainsKey("factorResult"))
                        {
                            pushResult = jsonTransaction.Value<string>("factorResult");
                        }

                        if (pushResult == null && jsonTransaction.ContainsKey("status"))
                        {
                            pushResult = jsonTransaction.Value<string>("status");
                        }

                        Console.WriteLine("Waiting for you to approve the Okta push notification on your device...");

                        try
                        {
                            Thread.Sleep(500);
                        }
                        catch (ThreadInterruptedException ex)
                        {
                            throw new SystemException(ex.Message, ex);
                        }

                        if (jsonTransaction.ContainsKey("sessionToken"))
                        {
                            sessionToken = jsonTransaction.Value<string>("sessionToken");
                        }
                    }
                }
            }

            if (sessionToken != null)
            {
                return sessionToken;
            }
            else
            {
                return pushResult;
            }
        }
    }
}
