﻿using OAuth.Helpers;
using OAuth.Signature;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OAuth
{
    public class OAuthMessageHandler : DelegatingHandler
    {
        private readonly string _apiKey;
        private readonly string _secret;
        private readonly string _authToken;
        private readonly string _authTokenSecret;
        private readonly IOAuthSignatureData _signatureDataProvider;
        private readonly KeyValuePair<string, string> _hmacSha1Param;
        private readonly KeyValuePair<string, string> _apiKeyParam;
        private readonly KeyValuePair<string, string> _authTokenParam;
        private readonly KeyValuePair<string, string> _oauthVersionParam;
        // the bytes used for the HMAC-SHA1
        private readonly byte[] _keyBytes;

        public OAuthMessageHandler(string apiKey, string secret, string authToken, string authTokenSecret) :
            this(apiKey, secret, authToken, authTokenSecret, new OAuthSignatureDataProvider(OAuthVersion.OneZero))
        {
        }

        public OAuthMessageHandler(string apiKey, string secret, string authToken, string authTokenSecret, OAuthVersion oauthVersion) :
            this(apiKey, secret, authToken, authTokenSecret, new OAuthSignatureDataProvider(oauthVersion))
        {
        }

        public OAuthMessageHandler(string apiKey, string secret, string authToken, string authTokenSecret, IOAuthSignatureData provider)
        {
            _apiKey = apiKey;
            _secret = secret;
            _authToken = authToken;
            _authTokenSecret = authTokenSecret;
            _signatureDataProvider = provider;

            _hmacSha1Param = new KeyValuePair<string, string>(Constants.oauth_signature_method, "HMAC-SHA1");
            _apiKeyParam = new KeyValuePair<string, string>(Constants.oauth_consumer_key, _apiKey);
            _authTokenParam = new KeyValuePair<string, string>(Constants.oauth_token, _authToken);

            // Construct the OAuthVersion parameter based on the requested version.
            _oauthVersionParam = new KeyValuePair<string, string>(Constants.oauth_version, _signatureDataProvider.GetOAuthVersion());
            _keyBytes = OAuthHelpers.CreateHashKeyBytes(_secret, _authTokenSecret);

            this.InnerHandler = new HttpClientHandler();
        }

        private async Task<string> GetAuthenticationHeaderForRequest(HttpRequestMessage request)
        {
            SortedSet<KeyValuePair<string, string>> parameters = new SortedSet<KeyValuePair<string, string>>(new OAuthParameterComparer())
            {
                // Re-use the parameters that don't change
                _apiKeyParam,
                _hmacSha1Param,
                _authTokenParam,

                // Add the parameters that are unique for each call
                new KeyValuePair<string,string>(Constants.oauth_nonce, _signatureDataProvider.GetNonce()),
                new KeyValuePair<string,string>(Constants.oauth_timestamp, _signatureDataProvider.GetTimeStamp()),
            };

            // if we have specified the OAuthVersion, add it!
            if (!string.IsNullOrEmpty(_oauthVersionParam.Value))
            {
                parameters.Add(_oauthVersionParam);
            }

            Uri requestUri = request.RequestUri;
            string baseUri = requestUri.OriginalString;

            // We need to handle the case where the request comes with query parameters, in URL or in body
            string queryString = string.Empty;
            if (!string.IsNullOrEmpty(requestUri.Query))
            {
                baseUri = requestUri.OriginalString.Replace(requestUri.Query, "");
                queryString = requestUri.Query;
            }

            // concatenate the content with the request content if the media type says we should.
            if (StringComparer.OrdinalIgnoreCase.Equals(request.Content?.Headers.ContentType?.MediaType, Constants.ContentType))
            {
                string requestContent = await request.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(queryString))
                {
                    queryString = requestContent;
                }
                else
                {
                    queryString = $"{queryString}&{requestContent}";
                }
            }

            if (!string.IsNullOrEmpty(queryString))
            {
                ParseParameters(parameters, queryString);
            }

            string baseString = OAuthHelpers.GenerateBaseString(baseUri, request.Method.ToString(), parameters);
            string sig = OAuthHelpers.EncodeValue(OAuthHelpers.GenerateHMACDigest(baseString, _keyBytes));

            parameters.Add(new KeyValuePair<string, string>(Constants.oauth_signature, sig));

            StringBuilder sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (param.Key.StartsWith("oauth"))
                {
                    // per spec (Section 3.5.1, point 2) https://tools.ietf.org/html/rfc5849#section-3.5.1:
                    // 2.Each parameter's name is immediately followed by an "=" character
                    // (ASCII code 61), a """ character (ASCII code 34), the parameter
                    // value(MAY be empty), and another """ character (ASCII code 34).

                    sb.AppendFormat("{0}=\"{1}\",", param.Key, System.Net.WebUtility.HtmlEncode(param.Value));
                }
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private void ParseParameters(SortedSet<KeyValuePair<string, string>> parameters, string queryString)
        {
            // if the first character of the string is a '?' then it came from the query string
            // otherwise it came from the request body.
            // Because we make certain assumptions about where a string starts, we need to account 
            // for those differences in the previous position initialization
            // 0 - if we have a query string, so we skip over the first character
            // -1 - if we don't have a query string '?'
            int previousPosition = queryString[0] == '?' ? 0 : -1; // beginning of the string

            queryString = Uri.UnescapeDataString(queryString);
            queryString = queryString.Replace('+', ' ');
            for (int outerIndex = 0; outerIndex <= queryString.Length; outerIndex++)
            {
                // we are going to try to parse parameters when we see a '&' or when we reach the end of the string.
                if (outerIndex == queryString.Length || queryString[outerIndex] == '&')
                {
                    // we are going to iterate on the current segment
                    int equalsPos = -1;
                    int segmentLength = outerIndex - previousPosition - 1;
                    string name = string.Empty, value = string.Empty;
                    for (int i = previousPosition + 1; i < previousPosition + segmentLength; i++)
                    {
                        // if we haven't found the equals yet, nothing to do.
                        if (queryString[i] != '=')
                            continue;

                        equalsPos = i;

                        int nameLength = i - 1 - previousPosition;
                        // up to this point, we have the name of the parameter.
                        name = queryString.Substring(previousPosition + 1, nameLength);
                        break;
                    }

                    // if we don't have a value, just a parameter
                    if (equalsPos == -1)
                    {
                        name = queryString.Substring(previousPosition + 1, segmentLength);
                    }
                    else
                    {
                        // the length of the value is from the equals to the end of the segment.
                        int valueLength = segmentLength - (equalsPos - previousPosition);
                        value = queryString.Substring(equalsPos + 1, valueLength);
                    }

                    parameters.Add(new KeyValuePair<string, string>(name, value));

                    //    // update the position of the last & or ? that we saw.
                    previousPosition = outerIndex;
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string header = await GetAuthenticationHeaderForRequest(request);
            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.OAuthAuthenticationHeader, header);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
