using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Okta
{
    public class OktaProxyConfig : ProxyConfig
    {
        public OktaConfig Okta { get; set; } = new();
    }
}
