using Newtonsoft.Json;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Security.Authentication;
using System.Text;

namespace Reforge.CliSupport
{
    public class HandleIdentityServerRoutes : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            var path = args.RequestUrl.AbsolutePath.ToLowerInvariant();

            switch (path)
            {
                case "/.well-known/openid-configuration":
                    HandleWellKnownConfigurationRoute(args);

                    break;

                case "/.well-known/openid-configuration/jwks":
                    HandleWellKnownConfigurationJwksRoute(args);

                    break;

                case "/connect/token":
                    HandleConnectTokenRoute(args);

                    break;
            }
        }

        private void HandleWellKnownConfigurationRoute(HttpRequestArgs args)
        {
            var scheme = args.HttpContext.Request.Headers[Sitecore.Configuration.Settings.LoadBalancingScheme];
            var host = args.HttpContext.Request.Headers[Sitecore.Configuration.Settings.LoadBalancingHost];
            string serverUrl;

            if (!string.IsNullOrEmpty(scheme) && !string.IsNullOrEmpty(host))
            {
                // Sitecore handles building the correct url when running behind reverse proxy
                serverUrl = Sitecore.Web.WebUtil.GetServerUrl();
            }
            else
            {
                // the HOST header contains both hostname+port when running in Docker container with exposed ports
                serverUrl = $"http://{args.HttpContext.Request.Headers["HOST"]}";
            }

            JsonResponse(args, new MinimalWellKnownOpenIdConfiguration(serverUrl));
        }

        private void HandleWellKnownConfigurationJwksRoute(HttpRequestArgs args)
        {
            JsonResponse(args, new { keys = new dynamic[0] });
        }

        private void HandleConnectTokenRoute(HttpRequestArgs args)
        {
            // validate input
            if (!args.HttpContext.Request.HttpMethod.Equals("POST"))
            {
                return;
            }

            var grantType = args.HttpContext.Request.Form["grant_type"];
            var clientId = args.HttpContext.Request.Form["client_id"];
            var clientSecret = args.HttpContext.Request.Form["client_secret"];

            if (string.IsNullOrEmpty(grantType) || grantType != "client_credentials" || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                JsonResponse(args, new { error = "invalid_input" });

                return;
            }

            // check that credentials works
            if (!AuthenticationManager.Login(clientId, clientSecret, false))
            {
                JsonResponse(args, new { error = "access_denied" });

                return;
            }

            // construct a access token as Base64 encoded JSON
            var accessTokenJsonString = JsonConvert.SerializeObject(new { username = clientId, password = clientSecret }, Formatting.None);
            var accessTokenBase64 = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(accessTokenJsonString));

            JsonResponse(args, new { access_token = accessTokenBase64, expires_in = 0 });
        }

        private void JsonResponse(HttpRequestArgs args, object @object)
        {
            args.HttpContext.Response.Write(JsonConvert.SerializeObject(@object, Formatting.None));
            args.HttpContext.Response.ContentEncoding = Encoding.UTF8;
            args.HttpContext.Response.ContentType = "application/json";
            args.HttpContext.Response.StatusCode = 200;
            args.HttpContext.ApplicationInstance.CompleteRequest();

            args.AbortPipeline();
        }

        class MinimalWellKnownOpenIdConfiguration
        {
            public MinimalWellKnownOpenIdConfiguration(string serverUrl)
            {
                issuer = serverUrl;
                jwks_uri = $"{serverUrl}/.well-known/openid-configuration/jwks";
                authorization_endpoint = $"{serverUrl}/connect/authorize";
                token_endpoint = $"{serverUrl}/connect/token";
                userinfo_endpoint = $"{serverUrl}/connect/userinfo";
                end_session_endpoint = $"{serverUrl}/connect/endsession";
                check_session_iframe = $"{serverUrl}/connect/checksession";
                revocation_endpoint = $"{serverUrl}/connect/revocation";
                introspection_endpoint = $"{serverUrl}/connect/introspect";
                device_authorization_endpoint = $"{serverUrl}/connect/deviceauthorization";
            }

            public string issuer { get; set; }
            public string jwks_uri { get; set; }
            public string authorization_endpoint { get; set; }
            public string token_endpoint { get; set; }
            public string userinfo_endpoint { get; set; }
            public string end_session_endpoint { get; set; }
            public string check_session_iframe { get; set; }
            public string revocation_endpoint { get; set; }
            public string introspection_endpoint { get; set; }
            public string device_authorization_endpoint { get; set; }
            public bool frontchannel_logout_supported => false;
            public bool frontchannel_logout_session_supported => false;
            public bool backchannel_logout_supported => false;
            public bool backchannel_logout_session_supported => false;
            public string[] scopes_supported => new string[0];
            public string[] claims_supported => new string[0];
            public string[] grant_types_supported => new string[] { "client_credentials" };
            public string[] response_types_supported => new string[] { "token" };
            public string[] response_modes_supported => new string[] { "form_post" };
            public string[] token_endpoint_auth_methods_supported => new string[] { "client_secret_post" };
            public string[] subject_types_supported => new string[] { "public" };
            public string[] id_token_signing_alg_values_supported => new string[] { "RS256" };
            public string[] code_challenge_methods_supported => new string[] { "plain", "S256" };
        }
    }
}