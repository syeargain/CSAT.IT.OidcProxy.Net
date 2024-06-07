using IdentityModel.Client;
using IdentityModel.OidcClient;

namespace OidcProxy.Net.Okta
{
    public interface IDiscoveryClient
    {
        Task<DiscoveryDocument> GetDiscoveryDocumentAsync(string discoveryAddress, DiscoveryPolicy policy);
        Task<ProviderInformation> GetProviderInformationAsync(string discoveryAddress, DiscoveryPolicy policy);
    }
}
