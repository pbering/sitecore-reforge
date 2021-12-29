using Newtonsoft.Json;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Security.Authentication;
using System.Text;

namespace Reforge.CliSupport
{
    public class HandleCliAuthentication : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            // only handle CLI requests
            if (!args.RequestUrl.AbsolutePath.Equals("/sitecore/api/management", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!args.HttpContext.Request.HttpMethod.Equals("POST"))
            {
                return;
            }

            // validate token data
            var accessTokenString = args.HttpContext.Request.Headers["Authorization"]?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(accessTokenString))
            {
                return;
            }

            byte[] accessTokenBytes;

            try
            {
                accessTokenBytes = System.Convert.FromBase64String(accessTokenString);
            }
            catch
            {
                AccessDeniedResponse(args);

                return;
            }

            var accessToken = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(accessTokenBytes));
            var userName = (string)accessToken.username;
            var password = (string)accessToken.password;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                AccessDeniedResponse(args);
            }

            // use token to login
            if (!AuthenticationManager.Login(userName, password, false))
            {
                AccessDeniedResponse(args);
            }
        }

        private void AccessDeniedResponse(HttpRequestArgs args)
        {
            args.HttpContext.Response.StatusCode = 403;
            args.HttpContext.ApplicationInstance.CompleteRequest();

            args.AbortPipeline();
        }
    }
}