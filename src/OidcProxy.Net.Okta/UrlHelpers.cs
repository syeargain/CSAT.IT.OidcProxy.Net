namespace OidcProxy.Net.Okta
{
    public static class UrlHelpers
    {
        public static string CreateIssuerUrl(string oktaDomain, string authorizationServerId)
        {
            if (string.IsNullOrEmpty(authorizationServerId))
            {
                return $"{EnsureTrailingSlash(oktaDomain)}oauth2";
            }

            return $"{EnsureTrailingSlash(oktaDomain)}oauth2/{authorizationServerId}";
        }

        public static string EnsureTrailingSlash(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                uri = string.Empty;
            }

            return uri.EndsWith("/")
                ? uri
                : $"{uri}/";
        }

        public static string EnsureNoTrailingSlash(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return uri.EndsWith("/")
                ? uri.Substring(0, uri.Length - 1)
                : uri;
        }
    }
}