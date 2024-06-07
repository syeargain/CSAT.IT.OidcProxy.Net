using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Okta
{
    public static class ModuleInitializer
    {
        public static void ConfigureOkta(this ProxyOptions options, IConfigurationSection configurationSection, string endpointName = ".auth")
             => ConfigureOkta(options, configurationSection.Get<OktaConfig>(), endpointName);

        public static void ConfigureOkta(this ProxyOptions options, OktaConfig config, string endpointName = ".auth")
        {
            if (!config.Validate(out var errors))
            {
                throw new NotSupportedException(string.Join(", ", errors));
            }

            options.RegisterIdentityProvider<OktaIdentityProvider, OktaConfig>(config, endpointName);
        }

        /// <summary>
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection, OktaProxyConfig config,
            Action<ProxyOptions>? configureOptions = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                    $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OktaProxyConfig)}`.");
            }

            var oidcConfig = config.Okta;
            var endpointName = config.EndpointName ?? ".auth";
            var routes = config.ReverseProxy?.Routes.ToRouteConfig();
            var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();

            if (oidcConfig == null)
            {
                throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                    $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OktaProxyConfig)}` " +
                    $"and provide a value for {nameof(OktaProxyConfig)}.{nameof(config.Okta)}.");
            }

            if (routes == null || !routes.Any())
            {
                throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                    $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OktaProxyConfig)}` " +
                    $"and provide a value for {nameof(OktaProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Routes)}.");
            }

            if (clusters == null || !clusters.Any())
            {
                throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                    $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OktaProxyConfig)}` " +
                    $"and provide a value for {nameof(OktaProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Clusters)}.");
            }

            return serviceCollection.AddOidcProxy(options =>
            {
                AssignIfNotNull(config.ErrorPage, options.SetAuthenticationErrorPage);
                AssignIfNotNull(config.LandingPage, options.SetLandingPage);
                AssignIfNotNull(config.CustomHostName, options.SetCustomHostName);
                AssignIfNotNull(config.CookieName, cookieName => options.CookieName = cookieName);
                AssignIfNotNull(config.NameClaim, nameClaim => options.NameClaim = nameClaim);
                AssignIfNotNull(config.RoleClaim, roleClaim => options.RoleClaim = roleClaim);

                options.EnableUserPreferredLandingPages = config.EnableUserPreferredLandingPages;
                options.AlwaysRedirectToHttps = !config.AlwaysRedirectToHttps.HasValue || config.AlwaysRedirectToHttps.Value;
                options.SetAllowedLandingPages(config.AllowedLandingPages);

                if (config.SessionIdleTimeout.HasValue)
                {
                    options.SessionIdleTimeout = config.SessionIdleTimeout.Value;
                }

                options.ConfigureOkta(oidcConfig, endpointName);

                options.ConfigureYarp(yarp => yarp.LoadFromMemory(routes, clusters));

                configureOptions?.Invoke(options);
            });
        }

        private static void AssignIfNotNull<T>(T? value, Action<T> @do)
        {
            if (value != null)
            {
                @do(value);
            }
        }
    }
}