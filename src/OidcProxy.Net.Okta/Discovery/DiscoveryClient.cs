using IdentityModel.Client;
using IdentityModel.OidcClient;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net.Okta
{
    /// <summary>
    ///
    /// </summary>
    public class DiscoveryClient : IDiscoveryClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClient"></param>
        public DiscoveryClient(ILogger logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        private async Task<DiscoveryDocumentResponse> GetDiscoveryResponseAsync(string discoveryAddress, DiscoveryPolicy policy)
        {
            DiscoveryDocumentRequest request = new DiscoveryDocumentRequest
            {
                Address = discoveryAddress,
                Policy = policy
            };

            var discoveryDocumentResponse = await _httpClient.GetDiscoveryDocumentAsync(request);
            if (discoveryDocumentResponse.IsError)
            {
                throw new InvalidOperationException($"Error loading discovery document response from {discoveryAddress} error message: {discoveryDocumentResponse.Error}");
            }

            return discoveryDocumentResponse;
        }

        public async Task<DiscoveryDocument> GetDiscoveryDocumentAsync(string discoveryAddress, DiscoveryPolicy policy)
        {
            DiscoveryDocumentRequest request = new DiscoveryDocumentRequest
            {
                Address = discoveryAddress,
                Policy = policy
            };

            var discoveryDocument = await _httpClient.GetDiscoveryDocumentAsync(request);

            return new DiscoveryDocument
            {
                authorization_endpoint = discoveryDocument.AuthorizeEndpoint,
                end_session_endpoint = discoveryDocument.EndSessionEndpoint,
                issuer = discoveryDocument.Issuer,
                jwks_uri = discoveryDocument.JwksUri,
                revocation_endpoint = discoveryDocument.RevocationEndpoint,
                token_endpoint = discoveryDocument.TokenEndpoint,
                userinfo_endpoint = discoveryDocument.UserInfoEndpoint,
                ErrorMessage = discoveryDocument.IsError ? discoveryDocument.Error : string.Empty
            };
        }

        public async Task<ProviderInformation> GetProviderInformationAsync(string discoveryAddress, DiscoveryPolicy policy)
        {
            var discoveryDocumentResponse = await GetDiscoveryResponseAsync(discoveryAddress, policy);
            return new ProviderInformation
            {
                IssuerName = discoveryDocumentResponse.Issuer,
                KeySet = discoveryDocumentResponse.KeySet,
                AuthorizeEndpoint = discoveryDocumentResponse.AuthorizeEndpoint,
                TokenEndpoint = discoveryDocumentResponse.TokenEndpoint,
                EndSessionEndpoint = discoveryDocumentResponse.EndSessionEndpoint,
                UserInfoEndpoint = discoveryDocumentResponse.UserInfoEndpoint,
                TokenEndPointAuthenticationMethods = discoveryDocumentResponse.TokenEndpointAuthenticationMethodsSupported,
            };
        }
    }
}