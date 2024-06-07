using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace OidcProxy.Net.Okta.Tests
{
    public class PolicyTests
    {
        [Fact]
        public void ShouldPassTheCorrectParametersToDiscoveryClient()
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            var config = builder.Configuration
                .GetSection("OidcProxy")
                .Get<OktaProxyConfig>();

            var app = builder.Build();
            var policy = config.Okta.GetDiscoveryPolicy();

            Assert.NotNull(policy);
            Assert.Contains(UrlHelpers.EnsureNoTrailingSlash(config.Okta.OktaDomain), policy.AdditionalEndpointBaseAddresses);
            Assert.Contains(UrlHelpers.CreateIssuerUrl(config.Okta.OktaDomain, string.Empty), policy.AdditionalEndpointBaseAddresses);
            Assert.Contains(UrlHelpers.EnsureNoTrailingSlash(config.Okta.Authority), policy.AdditionalEndpointBaseAddresses);
            Assert.Contains("registration_endpoint", policy.EndpointValidationExcludeList);

        }

    }
}
