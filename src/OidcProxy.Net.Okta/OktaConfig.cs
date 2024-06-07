using IdentityModel.Client;

namespace OidcProxy.Net.Okta
{
    public class OktaConfig
    {
        public string ClientId { get; set; } = string.Empty;

        public string ClientSecret { get; set; } = string.Empty;

        private string _oktaDomain = string.Empty;

        public string OktaDomain
        {
            get { return _oktaDomain; }

            set
            {
                _oktaDomain = value;
                Authority = UrlHelpers.CreateIssuerUrl(_oktaDomain, _authorizationServerId);
            }
        }

        private string _authorizationServerId = string.Empty;

        public string AuthorizationServerId
        {
            get { return _authorizationServerId; }
            set
            {
                _authorizationServerId = value;
                Authority = UrlHelpers.CreateIssuerUrl(_oktaDomain, _authorizationServerId);
            }
        }

        public string Authority { get; set; } = string.Empty;

        public string OpendIdDiscoveryEndpoint { get; set; } = $".well-known/openid-configuration";

        public DiscoveryPolicy GetDiscoveryPolicy()
        {
            var policy = new DiscoveryPolicy
            {
                // Okta Org AS must be included to avoid https://stackoverflow.com/questions/56459997/endpoint-belongs-to-different-authority
                //include both org and custom endpoint
                AdditionalEndpointBaseAddresses = GetAdditionalEndpoints(),
                EndpointValidationExcludeList = GetEndpointEndpointExclusions,
            };

            return policy;
        }

        public string GetOpenIdDiscoveryEndpoint()
        {
            return $"{UrlHelpers.CreateIssuerUrl(OktaDomain, AuthorizationServerId)}/" + $"{OpendIdDiscoveryEndpoint.TrimStart('/')}";
        }

        private List<string> GetAdditionalEndpoints()
                    => new List<string> {
                        $"{OktaDomain.TrimEnd('/')}",
                        $"{UrlHelpers.CreateIssuerUrl(OktaDomain, string.Empty)}",
                        $"{UrlHelpers.CreateIssuerUrl(OktaDomain, AuthorizationServerId)}" };

        public List<string> GetEndpointEndpointExclusions => new List<string> { "registration_endpoint" };
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public string PostLogoutRedirectEndpoint { get; set; } = "/";

        public virtual bool Validate(out IEnumerable<string> errors)
        {
            var results = new List<string>();
            if (string.IsNullOrEmpty(ClientId))
            {
                results.Add("GCN-O-e9ba6693bb0e: Unable to start OidcProxy.Net. Invalid client_id. " +
                            "Configure the client_id in the appsettings.json or program.cs file and try again. " +
                            "More info: https://bff.gocloudnative.org/errors/gcn-o-e9ba6693bb0e");
            }

            if (string.IsNullOrEmpty(ClientSecret))
            {
                results.Add("GCN-O-427413a281d9: Unable to start OidcProxy.Net. Invalid client_secret. " +
                            "Configure the client_secret in the appsettings.json or program.cs file and try again. " +
                            "More info: https://bff.gocloudnative.org/errors/gcn-o-427413a281d9");
            }

            if (string.IsNullOrEmpty(AuthorizationServerId))
            {
                results.Add("GCN-O-e9ba6693bb0e: Unable to start OidcProxy.Net. Invalid Authorization Server Name. " +
                            "Configure the authentication_server_name in the appsettings.json or program.cs file and try again. " +
                            "More info:");
            }

            if (string.IsNullOrEmpty(OktaDomain))
            {
                results.Add("Your Okta URL is missing. " +
                    "You can copy your domain from the Okta Developer Console. " +
                    "Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }

            if (!string.IsNullOrEmpty(OktaDomain) && !OktaDomain.StartsWith("https://"))
            {
                results.Add($"Your Okta URL must start with https. Current value: {OktaDomain}." +
                    "You can copy your domain from the Okta Developer Console. " +
                    "Follow these instructions to find it: https://bit.ly/finding-okta-domain");
            }

            //var urlRegex = @"^https?:\/\/";
            //if (OktaDomain == null || AuthorizationServerId == null || !Regex.IsMatch(OktaDomain, urlRegex))
            //{
            //    results.Add("GCN-O-e0180c31edd7: Unable to start OidcProxy.Net. Invalid authority. " +
            //                "Configure OktaDomain & AuthorizationServerand  in the appsettings.json or program.cs file and try again. " +
            //                "More info: https://bff.gocloudnative.org/errors/gcn-o-e0180c31edd7");
            //}

            errors = results;
            return !results.Any();
        }
    }
}