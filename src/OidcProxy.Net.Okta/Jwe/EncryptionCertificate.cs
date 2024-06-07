﻿using Jose;
using OidcProxy.Net.OpenIdConnect;
using System.Security.Cryptography.X509Certificates;

namespace OidcProxy.Net.Okta.Jwe
{
    public sealed class EncryptionCertificate(X509Certificate2 certificate) : IJweEncryptionKey
    {
        public string Decrypt(string token)
        {
            var privateKey = certificate.GetRSAPrivateKey();
            if (privateKey == null)
            {
                throw new NotSupportedException("Failed to decrypt JWE. " +
                                                "The provided certificate does not have a private key. " +
                                                "The private key of the certificate is required to decrypt the JWE. " +
                                                "Provide a certificate with a private key.");
            }

            var jweToken = JWE.Decrypt(token, privateKey);
            return jweToken.Plaintext;
        }
    }
}
