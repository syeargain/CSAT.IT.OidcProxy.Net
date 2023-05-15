using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.IdentityProviders;

public interface IIdentityProvider
{
    public Task<AuthorizeRequest> GetAuthorizeUrlAsync(HttpContext context);

    public Task<TokenResponse> GetTokenAsync(HttpContext context, string codeVerifier);
}