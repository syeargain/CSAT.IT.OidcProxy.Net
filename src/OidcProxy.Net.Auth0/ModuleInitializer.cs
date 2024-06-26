using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Auth0;

public static class ModuleInitializer
{
    public static void ConfigureAuth0(this ProxyOptions options, IConfigurationSection configurationSection, string endpointName = ".auth")
        => ConfigureAuth0(options, configurationSection.Get<Auth0Config>(), endpointName);

    public static void ConfigureAuth0(this ProxyOptions options, Auth0Config config, string endpointName = ".auth")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }

        options.RegisterIdentityProvider<Auth0IdentityProvider, Auth0Config>(config, endpointName);
    }

    /// <summary>
    /// Initialises the BFF. Also use app.UseAuth0Proxy();
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuth0Proxy(this IServiceCollection serviceCollection, Auth0ProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(Auth0ProxyConfig)}`.");
        }

        var auth0Config = config.Auth0;
        var endpointName = config.EndpointName ?? ".auth";
        var routes = config.ReverseProxy?.Routes.ToRouteConfig();
        var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();

        if (auth0Config == null)
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(Auth0ProxyConfig)}` " +
                $"and provide a value for {nameof(Auth0ProxyConfig)}.{nameof(config.Auth0)}.");
        }

        if (routes == null || !routes.Any())
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(Auth0ProxyConfig)}` " +
                $"and provide a value for {nameof(Auth0ProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Routes)}.");
        }

        if (clusters == null || !clusters.Any())
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(Auth0ProxyConfig)}` " +
                $"and provide a value for {nameof(Auth0ProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Clusters)}.");
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
            options.AllowAnonymousAccess = !config.AllowAnonymousAccess.HasValue || config.AllowAnonymousAccess.Value;
            options.SetAllowedLandingPages(config.AllowedLandingPages);

            if (config.SessionIdleTimeout.HasValue)
            {
                options.SessionIdleTimeout = config.SessionIdleTimeout.Value;
            }

            configureOptions?.Invoke(options);

            options.ConfigureAuth0(auth0Config, endpointName);

            options.ConfigureYarp(yarp => yarp.LoadFromMemory(routes, clusters));
        });
    }

    public static void UseAuth0Proxy(this WebApplication app) => app.UseOidcProxy();

    private static void AssignIfNotNull<T>(T? value, Action<T> @do)
    {
        if (value != null)
        {
            @do(value);
        }
    }
}