namespace OidcProxy.Net.Okta
{
    public class DiscoveryDocument
    {
        public string? issuer { get; set; }

        public string? authorization_endpoint { get; set; }

        public string? token_endpoint { get; set; }

        public string? userinfo_endpoint { get; set; }

        public string? jwks_uri { get; set; }

        public string? revocation_endpoint { get; set; }

        public string? end_session_endpoint { get; set; }

        public bool IsValid
        {
            get { return ErrorMessage == string.Empty; }
        }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
