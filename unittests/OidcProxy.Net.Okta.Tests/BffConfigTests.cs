using FluentAssertions;
using Newtonsoft.Json;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Okta.Tests
{
    public class OpenIdConnectBffConfigurationTests
    {
        private const string json = @"{
        ""Okta"": {
        ""ClientId"": ""0oaghusiei1wX2Fac5d7"",
        ""ClientSecret"": ""UIpiNaUvq3xUBnKChxMZG78lMrx9RtLiSMYUP9laisG81clIpLmhVeiXwUOqFBDj"",
        ""OktaDomain"": ""https://dev-33858045.okta.com"",
        ""AuthorizationServerId"": ""default"",
        ""Scopes"": [""openid"",""profile"",""offline_access""]
            },
        ""ErrorPage"": ""/error.aspx"",
        ""LandingPage"": ""/welcome.aspx"",
        ""RoleClaim"": ""role"",
        ""NameClaim"": ""name"",
        ""CustomHostName"": ""www.foobar.org"",
        ""CookieName"": ""bff.custom.cookie"",
        ""SessionIdleTimeout"": ""00:30:00"",
        ""PostLogoutRedirectEndpoint"": ""bye.aspx"",
        ""EndpointName"": ""auth"",
        ""ReverseProxy"": {
            ""Routes"": {
                ""apiroute"": {
                    ""ClusterId"": ""apicluster"",
                    ""Match"": {
                        ""Path"": ""/api/{*any}""
                    }
                }
            },
            ""Clusters"": {
                ""apicluster"": {
                    ""Destinations"": {
                        ""api/node1"": {
                            ""Address"": ""http://localhost:8080/""
                        }
                    }
                }
            }
        }
    }
    ";

        private readonly OktaProxyConfig? _deserializedObject;

        public OpenIdConnectBffConfigurationTests()
        {
            _deserializedObject = JsonConvert.DeserializeObject<OktaProxyConfig>(json);
        }

        [Fact]
        public void ItShouldReadOidcProperties()
        {
            _deserializedObject?.Okta.ClientId.Should().NotBeNullOrEmpty();
            _deserializedObject?.Okta.ClientSecret.Should().NotBeNullOrEmpty();
            _deserializedObject?.Okta.OktaDomain.Should().NotBeNullOrEmpty();
            _deserializedObject?.Okta.AuthorizationServerId.Should().NotBeNullOrEmpty();
            _deserializedObject?.Okta.Scopes.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ItShouldReadBffProperties()
        {
            _deserializedObject?.ErrorPage.Should().NotBeNullOrEmpty();
            _deserializedObject?.LandingPage.Should().NotBeNullOrEmpty();
            _deserializedObject?.NameClaim.Should().NotBeNullOrEmpty();
            _deserializedObject?.RoleClaim.Should().NotBeNullOrEmpty();
            _deserializedObject?.CustomHostName.Should().NotBeNull();
            _deserializedObject?.CookieName.Should().NotBeNullOrEmpty();
            _deserializedObject?.SessionIdleTimeout.Should().NotBe(TimeSpan.Zero);
        }

        [Fact]
        public void ItShouldReadClusters()
        {
            var clusters = _deserializedObject?.ReverseProxy.Clusters.ToClusterConfig();
            clusters.Should().NotBeEmpty();

            var cluster = clusters.FirstOrDefault();
            cluster.ClusterId.Should().Be("apicluster");
            cluster.Destinations.Should().NotBeEmpty();
        }

        [Fact]
        public void ItShouldReadRoutes()
        {
            var routes = _deserializedObject?.ReverseProxy.Routes.ToRouteConfig();
            routes.Should().NotBeEmpty();

            var route = routes.FirstOrDefault();
            route.RouteId.Should().Be("apiroute");
            route.Match.Path.Should().Be("/api/{*any}");
            route.ClusterId.Should().Be("apicluster");
        }

        [Fact]
        public void DiscoveryEndpointShouldContainDomainAndAuthServer()
        {
            _deserializedObject.Okta.GetOpenIdDiscoveryEndpoint().Should().ContainAll(
                new List<string> {
                    _deserializedObject.Okta.AuthorizationServerId,
                    _deserializedObject.Okta.OktaDomain
                });
        }
    }
}