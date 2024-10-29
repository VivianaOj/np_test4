using Nop.Plugin.Misc.NetSuiteConnector.Services.Common.OAuthBase;
using System;
using System.Security.Cryptography;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public interface IOAuthBaseHelper
    {
        string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters);

        string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash);

        string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters);

        string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string httpMethod, string timeStamp, string nonce, Common.OAuthBase.SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters);

        string GenerateTimeStamp();

        string GenerateNonce();

        string UrlEncode(string value);
    }
}
