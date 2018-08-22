using System;
using System.Net;
using System.Text;
using System.Threading;

using Supremes;
using Supremes.Nodes;

using okta_aws_cli.net.Authentication;
using okta_aws_cli.net.Helpers;

namespace okta_aws_cli.net.Saml
{
    public sealed class OktaSaml
    {
        private OktaAwsCliEnvironment environment;

        public OktaSaml(OktaAwsCliEnvironment environment)
        {
            this.environment = environment;
        }

        public string GetSamlResponse()
        {
            var authentication = new OktaAuthentication(environment);

            if (this.ReuseSession())
            {
                return GetSamlResponseForAwsRefresh();
            }
            else if (this.environment.browserAuth)
            {
                try
                {
                    throw new NotImplementedException("Not implemented for now");
                    //return BrowserAuthentication.Login(environment);
                }
                catch (ThreadInterruptedException e)
                {
                    throw new SystemException(e.Message, e);
                }
            }
            else
            {
                string oktaSessionToken = authentication.GetOktaSessionToken();
                return GetSamlResponseForAws(oktaSessionToken);
            }
        }

        private string GetSamlResponseForAws(string oktaSessionToken)
        {
            Document document = LaunchOktaAwsAppWithSessionToken(environment.oktaAwsAppUrl, oktaSessionToken);
            Elements samlResponseInputElements = document.Select("form input[name=SAMLResponse]");

            if (samlResponseInputElements.Count == 0)
            {
                throw new SystemException("You do not have access to AWS through Okta. \nPlease contact your administrator.");
            }

            return samlResponseInputElements.Attr("value");
        }

        private string GetSamlResponseForAwsRefresh()
        {
            Document document = LaunchOktaAwsApp(environment.oktaAwsAppUrl);
            Elements samlResponseInputElements = document.Select("form input[name=SAMLResponse]");

            if (samlResponseInputElements.Count == 0)
            {
                throw new SystemException("You do not have access to AWS through Okta. \nPlease contact your administrator.");
            }

            return samlResponseInputElements.Attr("value");
        }

        private Document LaunchOktaAwsAppWithSessionToken(string appUrl, string oktaSessionToken)
        {
            return LaunchOktaAwsApp(appUrl + "?onetimetoken=" + oktaSessionToken);
        }

        private Document LaunchOktaAwsApp(string appUrl)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(appUrl);
            httpRequest.CookieContainer = CookieHelper.LoadCookies(environment);

            using (var oktaAwsAppResponse = (HttpWebResponse)httpRequest.GetResponse()) {

                if (oktaAwsAppResponse.StatusCode >= HttpStatusCode.InternalServerError)
                {
                    throw new SystemException("Server error when loading Okta AWS App: " + oktaAwsAppResponse.StatusCode);
                }
                else if (oktaAwsAppResponse.StatusCode >= HttpStatusCode.BadRequest)
                {
                    throw new SystemException("Client error when loading Okta AWS App: " + oktaAwsAppResponse.StatusCode);
                }

                CookieHelper.StoreCookies(httpRequest.CookieContainer, appUrl);

                return Dcsoup.Parse(oktaAwsAppResponse.GetResponseStream(), Encoding.UTF8.EncodingName, appUrl);
            }
        }

        private bool ReuseSession()
        {
            var cookieStore = CookieHelper.LoadCookies(environment);
            
            var sidCookie = cookieStore.GetCookies(environment.OktaAwsAppUri)["sid"];
            if (sidCookie == null || string.IsNullOrWhiteSpace(sidCookie.Value))
            {
                return false;
            }

            var refreshUrl = $"https://{environment.oktaOrg}/api/v1/sessions/me/lifecycle/refresh";
            var httpRequest = (HttpWebRequest)WebRequest.Create(refreshUrl);
            httpRequest.Accept = "application/json";
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            httpRequest.Headers.Add("Cookie", "sid=" + sidCookie);

            try
            {
                using (var response = (HttpWebResponse)httpRequest.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
