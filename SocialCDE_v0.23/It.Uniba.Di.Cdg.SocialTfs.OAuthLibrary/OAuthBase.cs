using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace It.Uniba.Di.Cdg.SocialTfs.OAuthLibrary
{
    /// <summary>
    /// Provides all the functionality required to make an OAuth request.
    /// </summary>
    public class OAuthBase
    {
        #region Enums

        /// <summary>
        /// Provides a predefined set of algorithms that are supported officially by the protocol.
        /// </summary>
        protected enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Generate pseudo-random number.
        /// </summary>
        protected Random random = new Random();

        #endregion

        #region Methods

        /// <summary>
        /// Provides an internal structure to sort the query parameter.
        /// </summary>
        protected class QueryParameter
        {
            private String name = null;
            private String value = null;

            public QueryParameter(String name, String value)
            {
                this.name = name;
                this.value = value;
            }

            public String Name
            {
                get { return name; }
            }

            public String Value
            {
                get { return value; }
            }
        }

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters.
        /// </summary>
        protected class QueryParameterComparer : IComparer<QueryParameter>
        {
            #region IComparer<QueryParameter> Members

            public int Compare(QueryParameter x, QueryParameter y)
            {
                if (x.Name == y.Name)
                {
                    return String.Compare(x.Value, y.Value);
                }
                else
                {
                    return String.Compare(x.Name, y.Name);
                }
            }

            #endregion
        }


        /// <summary>
        /// Helper function to compute a hash value.
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algoirhtm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function.</param>
        /// <param name="data">The data to hash.</param>
        /// <returns>a Base64 String of the hash value.</returns>
        private String ComputeHash(HashAlgorithm hashAlgorithm, String data)
        {
            if (hashAlgorithm == null) throw new ArgumentNullException("hashAlgorithm");
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException("data");

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Internal function to cut out all non oauth query String parameters.
        /// </summary>
        /// <param name="parameters">The query String part of the Url.</param>
        /// <returns>A list of QueryParameter each containing the parameter name and value.</returns>
        private List<QueryParameter> GetQueryParameters(String parameters)
        {
            if (parameters.StartsWith("?"))
                parameters = parameters.Remove(0, 1);

            parameters = parameters.Replace("?", "&");

            List<QueryParameter> result = new List<QueryParameter>();

            if (!String.IsNullOrEmpty(parameters))
            {
                String[] p = parameters.Split('&');

                foreach (String s in p)
                {
                    if (!String.IsNullOrEmpty(s) && !s.StartsWith(OAuthConstants.Default.OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            String[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                            result.Add(new QueryParameter(s, String.Empty));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth.
        /// </summary>
        /// <param name="value">The value to Url encode.</param>
        /// <returns>Returns a Url encoded String.</returns>
        protected String UrlEncode(String value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (OAuthConstants.Default.unreservedChars.IndexOf(symbol) != -1)
                    result.Append(symbol);
                else
                    //some symbols produce > 2 char values so the system urlencoder must be used to get the correct data
                    if (String.Format("{0:X2}", (int)symbol).Length > 3)
                        result.Append(HttpUtility.UrlEncode(value.Substring(value.IndexOf(symbol), 1)).ToUpper());
                    else
                        result.Append('%' + String.Format("{0:X2}", (int)symbol));
            }

            return result.ToString();
        }

        /// <summary>
        /// Normalizes the request parameters according to the spec.
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted.</param>
        /// <returns>a String representing the normalized parameters.</returns>
        protected String NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;

            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                    sb.Append("&");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the signature base that is used to produce the signature.
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters.</param>
        /// <param name="consumerKey">The consumer key.</param>        
        /// <param name="token">The token, if available. If not available pass null or an empty String.</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty String.</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb.</param>
        /// <param name="signatureType">The signature type. To use the default values use.<see cref="OAuthBase.SignatureTypes">OAuthBase.SignatureTypes</see>.</param>
        /// <returns>The signature base.</returns>
        protected String GenerateSignatureBase(Uri url, String consumerKey, String token, String tokenSecret, String verifier, String httpMethod, String timeStamp, String nonce, String signatureType, out String normalizedUrl, out String normalizedRequestParameters)
        {
            if (token == null) token = String.Empty;
            if (tokenSecret == null) tokenSecret = String.Empty;
            if (String.IsNullOrEmpty(consumerKey)) throw new ArgumentNullException("consumerKey");
            if (String.IsNullOrEmpty(httpMethod)) throw new ArgumentNullException("httpMethod");
            if (String.IsNullOrEmpty(signatureType)) throw new ArgumentNullException("signatureType");

            normalizedUrl = null;
            normalizedRequestParameters = null;

            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthVersionKey, OAuthConstants.Default.OAuthVersion));
            parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthConsumerKeyKey, consumerKey));

            if (String.IsNullOrEmpty(token))
                parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthCallbackKey, "oob"));

            if (!String.IsNullOrEmpty(token))
                parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthTokenKey, token));

            if (!String.IsNullOrEmpty(verifier))
                parameters.Add(new QueryParameter(OAuthConstants.Default.OAuthVerifier, verifier));

            parameters.Sort(new QueryParameterComparer());

            normalizedUrl = String.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
                normalizedUrl += ":" + url.Port;

            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>
        /// Generate the signature value based on the given signature base and hash algorithm.
        /// </summary>
        /// <param name="signatureBase">The signature based as produced by the GenerateSignatureBase method or by any other means.</param>
        /// <param name="hash">The hash algorithm used to perform the hashing. If the hashing algorithm requires initialization or a key it should be set prior to calling this method.</param>
        /// <returns>A base64 String of the hash value.</returns>
        protected String GenerateSignatureUsingHash(String signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm.
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer seceret.</param>
        /// <param name="token">The token, if available. If not available pass null or an empty String.</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty String.</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb.</param>
        /// <returns>A base64 String of the hash value.</returns>
        protected String GenerateSignature(Uri url, String consumerKey, String consumerSecret, String token, String tokenSecret, String verifier, String httpMethod, String timeStamp, String nonce, out String normalizedUrl, out String normalizedRequestParameters)
        {
            return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, verifier, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
        }

        /// <summary>
        /// Generates a signature using the specified signatureType.
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer seceret.</param>
        /// <param name="token">The token, if available. If not available pass null or an empty String.</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty String</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb</param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <returns>A base64 String of the hash value</returns>
        protected String GenerateSignature(Uri url, String consumerKey, String consumerSecret, String token, String tokenSecret, String verifier, String httpMethod, String timeStamp, String nonce, SignatureTypes signatureType, out String normalizedUrl, out String normalizedRequestParameters)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    return HttpUtility.UrlEncode(String.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMACSHA1:
                    String signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, verifier, httpMethod, timeStamp, nonce, OAuthConstants.Default.HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(String.Format("{0}&{1}", UrlEncode(consumerSecret), String.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                case SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        /// <summary>
        /// Generate the timestamp for the signature.     
        /// </summary>
        /// <returns>The timestamp in String format.</returns>
        protected virtual String GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Generate a nonce.
        /// </summary>
        /// <returns>The nounce in String format.</returns>
        protected virtual String GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }

        #endregion
    }
}