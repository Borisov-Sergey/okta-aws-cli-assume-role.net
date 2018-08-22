using System;
using System.Collections.Generic;
using System.Net;

namespace okta_aws_cli.net.Helpers
{
    public sealed class CookieHelper
    {
        /**
         * Get the path for the cookies file
         * 
         * @return A {@link Path} to the cookies.properties file
         * @throws IOException
         */
        private static string GetCookiesFilePath()
        {
            var filePath = FileHelper.GetFilePath(FileHelper.GetOktaDirectory(), "cookies.properties");
            return filePath;
        }

        public static CookieContainer ParseCookies(List<String> cookieHeaders)
        {
            var cookieStore = new CookieContainer();

            foreach (var cookieHeader in cookieHeaders)
            {
                foreach (var cookieParts in cookieHeader.Split(';'))
                {
                    var indexOfEquals = cookieParts.IndexOf('=');
                    var name = cookieParts.Substring(0, indexOfEquals);
                    var value = cookieParts.Substring(indexOfEquals + 1);

                    var cookie = new Cookie(name, value);
                    cookieStore.Add(cookie);
                }
            }

            return cookieStore;
        }

        public static CookieContainer LoadCookies(OktaAwsCliEnvironment environment)
        {
            var cookieStore = new CookieContainer();
            var loadedProperties = new Properties(GetCookiesFilePath());

            foreach (var property in loadedProperties.List)
            {
                var cookie = new Cookie(property.Key, property.Value);
                cookie.Domain = environment.oktaOrg;

                cookieStore.Add(cookie);
            }

            return cookieStore;
        }

        public static void StoreCookies(CookieContainer cookieStore, string url)
        {
            var properties = new Properties(GetCookiesFilePath());

            foreach (KeyValuePair<string, string> cookie in cookieStore.GetCookies(new Uri(url)))
            {
                properties.SetProperty(cookie.Key, cookie.Value);
            }

            properties.Save(GetCookiesFilePath());
        }

        public static void ClearCookies()
        {
            FileHelper.DeleteFile(GetCookiesFilePath());
        }
    }
}
