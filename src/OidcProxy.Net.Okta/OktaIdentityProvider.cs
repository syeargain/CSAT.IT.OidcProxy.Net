using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Caching.Memory;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.OpenIdConnect;
using System.Net;
using System.Web;
using ILogger = OidcProxy.Net.Logging.ILogger;
using TokenResponse = OidcProxy.Net.IdentityProviders.TokenResponse;

namespace OidcProxy.Net.Okta
{
    public class OktaIdentityProvider(
        ILogger logger,
        IMemoryCache cache,
        HttpClient httpClient,
        OktaConfig configuration) : IIdentityProvider
    {
        private IDiscoveryClient? _discoveryClient = null;

        public IDiscoveryClient GetDiscoveryClient()
        {
            if (_discoveryClient == null) _discoveryClient = new DiscoveryClient(logger, httpClient);
            return _discoveryClient;
        }

        public void SetDiscoveryClient(IDiscoveryClient newClient)
        {
            _discoveryClient = newClient;
        }

        protected virtual string OpenIdDiscoveryEndpointAddress => configuration.GetOpenIdDiscoveryEndpoint();

        public virtual DiscoveryPolicy GetDiscoveryPolicy()
        {
            return configuration.GetDiscoveryPolicy();
        }

        public virtual async Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
        {
            await logger.InformAsync($"Get authorize url for redirect: {redirectUri ?? string.Empty}");
            await logger.InformAsync("Get provider info..");
            var providerInfo = await GetDiscoveryClient().GetProviderInformationAsync(configuration.Authority, GetDiscoveryPolicy());
            await logger.InformAsync("Initialize OidcClient..");
            var client = new OidcClient(new OidcClientOptions
            {
                Authority = configuration.Authority,
                ClientId = configuration.ClientId,
                ClientSecret = configuration.ClientSecret,
                RedirectUri = redirectUri,
                ProviderInformation = providerInfo
            });
            await logger.InformAsync("Prepare login..");
            var request = await client.PrepareLoginAsync();
            var scopes = new Scopes(configuration.Scopes);
            var scopesParameter = $"scope={string.Join("%20", scopes)}";
            var startUrl = $"{request.StartUrl}&{scopesParameter}";
            await logger.InformAsync($"end okta provide authorize url startUrl: {startUrl ?? string.Empty}");
            ArgumentNullException.ThrowIfNullOrEmpty(startUrl);
            return new AuthorizeRequest(new Uri(startUrl), request.CodeVerifier);
        }

        public async Task<Uri> GetEndSessionEndpointAsync(string? idToken, string baseAddress)
        {
            // Determine redirect URL
            await logger.InformAsync($"GetEndSessionEndpointAsync redirect: {baseAddress ?? string.Empty}");
            var logOutRedirectEndpoint = configuration.PostLogoutRedirectEndpoint.StartsWith('/')
                ? configuration.PostLogoutRedirectEndpoint
                : $"/{configuration.PostLogoutRedirectEndpoint}";

            await logger.InformAsync($"GetEndSessionEndpointAsync logOutRedirectEndpoint: {logOutRedirectEndpoint ?? string.Empty}");
            var redirectUrl = $"{baseAddress}{logOutRedirectEndpoint}";

            await logger.InformAsync($"GetEndSessionEndpointAsync redirectUrl: {redirectUrl ?? string.Empty}");

            ArgumentNullException.ThrowIfNullOrEmpty(redirectUrl);
            return await BuildEndSessionUri(idToken, redirectUrl);
        }

        protected virtual async Task<Uri> BuildEndSessionUri(string? idToken, string redirectUri)
        {
            //unchanged copy of OpenIdProvider
            var openIdConfiguration = await GetDiscoveryDocument();

            var endSessionUrEndpoint = openIdConfiguration?.end_session_endpoint;
            if (endSessionUrEndpoint == null)
            {
                throw new NotSupportedException($"Invalid OpenId configuration. OpenId Configuration MUST contain a value for end_session_ endpoint. (https://openid.net/specs/openid-connect-session-1_0-17.html#OPMetadata)");
            }

            var urlEncodedRedirectUri = HttpUtility.UrlEncode(redirectUri);
            var endSessionUrl = $"{endSessionUrEndpoint}?id_token_hint={idToken}&post_logout_redirect_uri={urlEncodedRedirectUri}";
            return new Uri(endSessionUrl);
        }

        public async Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier, string traceIdentifier)
        {
            await logger.InformAsync($"GetTokenAsync for trace: {traceIdentifier}");
            await logger.InformAsync($"Get discovery document: {traceIdentifier}");
            var wellKnown = await GetDiscoveryDocument();
            ArgumentNullException.ThrowIfNull(wellKnown);
            if (wellKnown.token_endpoint == null)
            {
                throw new ApplicationException(
                    "Unable to exchange code for access_token. The well-known/openid-configuration" +
                    "document does not contain a token endpoint.");
            }

            var scopes = new Scopes(configuration.Scopes);

            await logger.InformAsync($"Request token: {traceIdentifier}");
            var response = await httpClient.RequestTokenAsync(new AuthorizationCodeTokenRequest
            {
                Address = wellKnown.token_endpoint,
                GrantType = OidcConstants.GrantTypes.AuthorizationCode,
                ClientId = configuration.ClientId,
                ClientSecret = configuration.ClientSecret,
                Parameters =
             {
                 { OidcConstants.TokenRequest.Code, code },
                 { OidcConstants.TokenRequest.Scope, string.Join(' ', scopes) },
                 { OidcConstants.TokenRequest.RedirectUri, redirectUri },
                 { OidcConstants.TokenRequest.CodeVerifier, codeVerifier },
             }
            });

            if (response.IsError)
            {
                await logger.InformAsync($"Error GetTokenAsync {response.Error}");
                throw new ApplicationException($"Unable to retrieve token. " +
                                               $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
            }

            await logger.InformAsync($"Queried token endpoint and obtained id_, access_, and refresh_tokens.");

            var expiryDate = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
            await logger.InformAsync($"End GetTokenAsync  for trace: {traceIdentifier}, expires: {expiryDate}");
            return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken, expiryDate);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string traceIdentifier)
        {
            var openIdConfiguration = await GetDiscoveryDocument();
            await logger.InformAsync($"Request token refresh: {traceIdentifier}");
            var response = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = openIdConfiguration?.token_endpoint,
                GrantType = OidcConstants.GrantTypes.RefreshToken,
                RefreshToken = refreshToken,
                ClientId = configuration.ClientId,
                ClientSecret = configuration.ClientSecret,
            });

            if (response.IsError)
            {
                throw new TokenRenewalFailedException($"Unable to retrieve token. " +
                                                      $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
            }

            await logger.InformAsync($"Queried /token endpoint (refresh grant) and obtained id_, access_, and refresh_tokens.");
            var expiresIn = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
            await logger.InformAsync($"Token expires in: {expiresIn} seconds");
            return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken, expiresIn);
        }

        public async Task RevokeAsync(string token, string traceIdentifier)
        {
            await logger.InformAsync($"Revoke token: {traceIdentifier}");
            var openIdConfiguration = await GetDiscoveryDocument();
            await logger.InformAsync($"Get response for: {traceIdentifier}");
            ArgumentNullException.ThrowIfNull(openIdConfiguration);
            var response = await httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = openIdConfiguration.revocation_endpoint,
                Token = token,
                ClientId = configuration.ClientId,
                ClientSecret = configuration.ClientSecret
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"Unable to revoke tokens. OIDC server responded {response.HttpStatusCode}:" +
                                               $" \r\n{response.Raw}");
            }
            else
            {
                await logger.InformAsync($"Revoke token api returned {response.HttpStatusCode}: {traceIdentifier}");
            }

            await logger.InformAsync($"Token revoked.");
        }

        private async Task<DiscoveryDocument?> GetDiscoveryDocument()
        {
            await logger.InformAsync($"Begin GetDiscoveryDocument");

            var endpointAddress = UrlHelpers.CreateIssuerUrl(configuration.OktaDomain, configuration.AuthorizationServerId);

            if (cache.TryGetValue(OpenIdDiscoveryEndpointAddress, out DiscoveryDocument? discoveryDocument))
            {
                await logger.InformAsync($"GetDiscoveryDocument - Cache Hit");
                return discoveryDocument as DiscoveryDocument;
            }

            await logger.InformAsync($"Call Discovery Client");

            discoveryDocument = await GetDiscoveryClient().GetDiscoveryDocumentAsync(endpointAddress, GetDiscoveryPolicy());

            if (discoveryDocument == null)
            {
                await logger.ErrorAsync($"Discovery Client Returned Null");

                throw new ApplicationException(
                    "Login failed. Unable to find a well-known/openid-configuration document " +
                    $"at {endpointAddress}");
            }
            else if (!discoveryDocument.IsValid)
            {
                throw new ApplicationException(
                    "Login failed. Invalid endpoint returned from " +
                    $"at {endpointAddress} with error " + discoveryDocument.ErrorMessage);
            }
            await logger.InformAsync($"End GetDiscoveryDocument");
            cache.Set(endpointAddress, discoveryDocument, TimeSpan.FromHours(1));
            return (DiscoveryDocument)discoveryDocument;
        }
    }
}