﻿//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc.Testing;
//using NSubstitute;
//using OidcProxy.Net.Logging;

//namespace OidcProxy.Net.Okta.Tests.OktaIdentityProviderTests
//{
//    public class GetTokenTests
//    {
//        private const string TraceIdentifier = "foo";
//        private readonly HttpClient _httpClient;
//        private readonly OktaConfig _config;
//        private readonly TestCache _cache;
//        private readonly ILogger _logger = Substitute.For<ILogger>();

//        public GetTokenTests()
//        {
//            _httpClient = new WebApplicationFactory<TestProgram>()
//                .CreateClient();

//            _config = new OktaConfig
//            {
//                OktaDomain = _httpClient.BaseAddress?.ToString(),
//                AuthorizationServerId = "default"
//            };

//            _cache = new TestCache();
//        }

//        [Fact]
//        public async Task ShouldApplyExpiresInValueFromTokenResponse()
//        {
//            TestProgram.AccessTokenResponse = @"{
//                ""access_token"":""MTQ0NjJkZmQ5OTM2NDE1ZTZjNGZmZjI3"",
//                ""id_token"":""MTQ0NjJkZmQ5OTM2NDE1ZTZjNGZmZjI3"",
//                ""token_type"":""Bearer"",
//                ""expires_in"":3600,
//                ""refresh_token"":""IwOGYzYTlmM2YxOTQ5MGE3YmNmMDFkNTVk"",
//                ""scope"":""create""
//            }";

//            //var AccessTokenResponse = File.ReadAllText("Content/AccessTokenResponse.json");

//            //TestProgram.AccessTokenResponse = File.ReadAllText("Content/AccessTokenResponse.json");
//            //var fakeCleint = Substitute.For<DiscoveryClient>();
//            var expected = DateTime.UtcNow.AddSeconds(3600);
//            var sut = new OktaIdentityProvider(_logger, _cache, _httpClient, _config);

//            var tokenResponse = await sut.GetTokenAsync("lorum", "ipsum", "dolar sit amet", TraceIdentifier);

//            var actual = tokenResponse.ExpiryDate - expected;

//            actual.TotalSeconds.Should().BeLessThan(10);
//        }
//    }
//}