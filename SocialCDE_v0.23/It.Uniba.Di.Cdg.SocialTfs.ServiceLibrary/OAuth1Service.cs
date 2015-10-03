using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Web;
using It.Uniba.Di.Cdg.SocialTfs.OAuthLibrary;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{
    /// <summary>
    /// Phases of OAuth authentication.
    /// </summary>
    public enum OAuth1Phase
    {
        RequestOAuthData,
        Authorize
    }

    internal class OAuth1Service : OAuth
    {
        /// <summary>
        /// Get the OAuth data (access token and access secret).
        /// </summary>
        /// <param name="requestToken">Access token for the request.</param>
        /// <param name="authorize">URI for the request.</param>
        /// <returns>OAuth data.</returns>
        internal OAuthAccessData GetOAuthData(String requestToken, String authorize)
        {
            Contract.Requires(!String.IsNullOrEmpty(requestToken));
            Contract.Requires(!String.IsNullOrEmpty(authorize));

            OAuthAccessData accessData = new OAuthAccessData();

            String response = GetRequest(requestToken);
            if (response.Length > 0)
            {
                //response contains token and token secret.
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    accessData.AccessToken = qs["oauth_token"];
                    accessData.RequestUri = authorize + "?oauth_token=" + qs["oauth_token"];
                }

                if (qs["oauth_token_secret"] != null)
                {
                    accessData.AccessSecret = qs["oauth_token_secret"];
                }
            }
            return accessData;
        }

        /// <summary>
        /// Authorize the application on the service for a specific user with the given "verifier".
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <param name="verifier">Verifier.</param>
        /// <returns>OAuth data.</returns>
        public OAuthAccessData Authorize(String accessToken, String verifier)
        {
            Contract.Requires(!String.IsNullOrEmpty(accessToken));

            this._verifier = verifier;
            String response = String.Empty;

            try
            {
                response = GetRequest(accessToken);
            }
            catch
            {
                return null;
            }

            if (response.Length > 0)
            {
                //Store the Token and Token Secret
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                    _accessToken = qs["oauth_token"];

                if (qs["oauth_token_secret"] != null)
                    _accessSecret = qs["oauth_token_secret"];
            }
            return new OAuthAccessData()
            {
                AccessToken = _accessToken,
                AccessSecret = _accessSecret
            };
        }
    }
}