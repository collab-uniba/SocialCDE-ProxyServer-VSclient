using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace It.Uniba.Di.Cdg.SocialTfs.OAuthLibrary
{
    /// <summary>
    /// Provides methods for sending web requests.
    /// </summary>
    public class OAuth : OAuthBase
    {
        #region Enums

        /// <summary>
        /// Represents the web request types.
        /// </summary>
        protected enum Method
        {
            GET,
            POST
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Root path for API of the service.
        /// </summary>
        protected String _host = "";

        /// <summary>
        /// The consumer key provided by the web application at the end of the registration process.
        /// </summary>
        protected String _consumerKey = "";

        /// <summary>
        /// The consumer secret provided by the web application at the end of the registration process.
        /// </summary>
        protected String _consumerSecret = "";

        /// <summary>
        /// The access token provided by the web application at the end of the authentication process.
        /// </summary>
        protected String _accessToken = "";

        /// <summary>
        /// The access secret provided by the web application at the end of the authentication process.
        /// </summary>
        protected String _accessSecret = "";

        /// <summary>
        /// The verifier pin provided by the web application during the authentication process.
        /// </summary>
        protected String _verifier = "";

        #endregion

        #region Methods

        /// <summary>
        /// Submit a web request using oAuth.
        /// </summary>
        /// <param name="method">GET or POST.</param>
        /// <param name="url">The full url, including the queryString.</param>
        /// <param name="postData">Data to post (queryString format).</param>
        /// <returns>The web server response.</returns>
        private String oAuthWebRequest(Method method, String url, String postData)
        {
            String outUrl = "";
            String queryString = "";

            //Setup postData for signing.
            //Add the postData to the queryString.
            if (method == Method.POST)
            {
                if (postData.Length > 0)
                {
                    //Decode the parameters and re-encode using the oAuth UrlEncode method.
                    NameValueCollection qs = HttpUtility.ParseQueryString(postData);
                    postData = "";

                    foreach (String key in qs.AllKeys)
                    {
                        if (postData.Length > 0)
                            postData += "&";

                        qs[key] = HttpUtility.UrlDecode(qs[key]);
                        qs[key] = this.UrlEncode(qs[key]);
                        postData += key + "=" + qs[key];
                    }

                    if (url.IndexOf("?") > 0)
                        url += "&";
                    else
                        url += "?";

                    url += postData;
                }
            }

            Uri uri = new Uri(url);

            String nonce = this.GenerateNonce();
            String timeStamp = this.GenerateTimeStamp();

            //Generate Signature
            String sig = this.GenerateSignature(uri,
                this._consumerKey,
                this._consumerSecret,
                this._accessToken,
                this._accessSecret,
                this._verifier,
                method.ToString(),
                timeStamp,
                nonce,
                out outUrl,
                out queryString);

            queryString += "&oauth_signature=" + HttpUtility.UrlEncode(sig);

            //Convert the queryString to postData
            if (method == Method.POST)
            {
                postData = queryString;
                queryString = "";
            }

            if (queryString.Length > 0)
                outUrl += "?";
            System.Diagnostics.Debug.WriteLine("query " + (outUrl + queryString));
            return WebRequest(method, outUrl + queryString, postData);
        }

        /// <summary>
        /// Web request wrapper.
        /// </summary>
        /// <param name="method">Http Method.</param>
        /// <param name="url">Full url to the web resource.</param>
        /// <param name="postData">Data to post in queryString format.</param>
        /// <returns>The web server response.</returns>
        private String WebRequest(Method method, String url, String postData)
        {
            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                //POST the data.
                using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                    requestWriter.Write(postData);
            }

            return WebResponseGet(webRequest);
        }

        /// <summary>
        /// Process the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        private String WebResponseGet(HttpWebRequest webRequest)
        {
            try
            {
                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                    return responseReader.ReadToEnd();
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        #endregion

        #region Utility

        protected String GetRequest(String url)
        {
            return oAuthWebRequest(Method.GET, url, String.Empty);
        }

        protected String PostRequest(String url, String postData)
        {
            return oAuthWebRequest(Method.POST, url, postData);
        }

        #endregion
    }
}
