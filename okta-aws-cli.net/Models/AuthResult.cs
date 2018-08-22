using System.Net;

namespace okta_aws_cli.net.Models
{
    public sealed class AuthResult
    {
        public string ResponseContent { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        public AuthResult(HttpStatusCode statusCode, string responseContent)
        {
            this.StatusCode = statusCode;
            this.ResponseContent = responseContent;
        }
    }
}
