using Jose;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Okta.Jwe
{
    public sealed class EncryptionKey(SymmetricSecurityKey key) : IJweEncryptionKey
    {
        public string Decrypt(string token)
        {
            var jweToken = JWE.Decrypt(token, key.Key);
            return jweToken.Plaintext;
        }
    }
}
