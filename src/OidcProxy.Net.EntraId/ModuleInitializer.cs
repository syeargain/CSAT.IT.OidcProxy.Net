﻿using OidcProxy.Net.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.EntraId;

public static class ModuleInitializer
{
    public static void ConfigureAzureAd(this ProxyOptions options, IConfigurationSection configurationSection, string endpointName = ".auth")
        => ConfigureAzureAd(options, configurationSection.Get<EntraIdConfig>(), endpointName);

    public static void ConfigureAzureAd(this ProxyOptions options, EntraIdConfig config, string endpointName = ".auth")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }
        
        options.RegisterIdentityProvider<AzureAdIdentityProvider, EntraIdConfig>(config, endpointName);
    }
    
    /// <summary>
    /// Initialises the BFF. Also use app.UseBff();
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection, EntraIdProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(EntraIdProxyConfig)}`.");
        }

        var aadConfig = config.EntraId;
        var endpointName = config.EndpointName ?? ".auth";
        var routes = config.ReverseProxy?.Routes.ToRouteConfig();
        var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();
        
        if (aadConfig == null)
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(EntraIdProxyConfig)}` " +
                $"and provide a value for {nameof(EntraIdProxyConfig)}.{nameof(config.EntraId)}.");
        }
        
        if (routes == null || !routes.Any())
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(EntraIdProxyConfig)}` " +
                $"and provide a value for {nameof(EntraIdProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Routes)}.");
        }
        
        if (clusters == null || !clusters.Any())
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(EntraIdProxyConfig)}` " +
                $"and provide a value for {nameof(EntraIdProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Clusters)}.");
        }

        return serviceCollection.AddOidcProxy(options =>
        {
            AssignIfNotNull(config.ErrorPage, options.SetAuthenticationErrorPage);
            AssignIfNotNull(config.LandingPage, options.SetLandingPage);
            AssignIfNotNull(config.CustomHostName, options.SetCustomHostName);
            AssignIfNotNull(config.SessionCookieName, cookieName => options.SessionCookieName = cookieName);
            
            if (config.SessionIdleTimeout.HasValue)
            {
                options.SessionIdleTimeout = config.SessionIdleTimeout.Value;
            }
            
            options.ConfigureAzureAd(aadConfig, endpointName);
        
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