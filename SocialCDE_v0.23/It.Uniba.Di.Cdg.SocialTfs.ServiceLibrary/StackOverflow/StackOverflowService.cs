using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Web;
using It.Uniba.Di.Cdg.SocialTfs.OAuthLibrary;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.IO;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.StackOverflow
{
    /// <summary>
    /// Rapresent the StackOverflow social network service.
    /// </summary>
    /// 
  

    class StackOverflowService : OAuth2Service, IService
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal StackOverflowService()
        {
            _host = null;
            _consumerKey = null;
            _consumerSecret = null;
            _accessToken = null;
            //  _accessSecret = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Root path for API of the service.</param>
        /// <param name="consumerKey">Consumer key of the service.</param>
        /// <param name="consumerSecret">Consumer secret of the service.</param>
        /// <param name="accessToken">Access token of the service.</param>
        /// <param name="accessSecret">Access secret of the service.</param>
        // internal StackOverflowService(String host, String consumerKey, String consumerSecret, String accessToken, String accessSecret)
        internal StackOverflowService(String host, String consumerKey, String consumerSecret, String accessToken)
        {
            _host = host;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            // _accessSecret = accessSecret;
        }

        #endregion

        #region IService

        int IService.Version
        {
            get { return 1; }
        }

        string IService.Name
        {
            get { return "StackOverflow"; }
        }

        IUser IService.VerifyCredential()
        {

            String jsonUser = WebRequestStackOverflow(_host + StackOverflowUri.Default.CREDENTIALS);
            jsonUser.ToString();
            jsonUser = jsonUser.Substring(10, jsonUser.Length - 10);
            jsonUser = jsonUser.Remove(jsonUser.Length - 2);
            return JsonConvert.DeserializeObject<StackOverflowUser>(jsonUser);
            

        }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();
            features.Add(FeaturesType.Avatar);
            features.Add(FeaturesType.Reputation);
            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();
            features.Add(FeaturesType.OAuth2);
            return features;
        }

        List<FeaturesType> IService.GetScoredFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();
            return features;
        }

        object IService.Get(FeaturesType feature, params object[] param)
        {
            object result = null;

            switch (feature)
            {
                
            case FeaturesType.Avatar:
                result = GetAvatarUri();
                break;
            case FeaturesType.Reputation:
                result = GetStackOverflowReputation();
                break;
            case FeaturesType.OAuth2:
                result = (param.Length > 1 ? GetOAuthData((string)param[0], (string)param[1], (string)param[2], (string)param[3], (string)param[4]) : GetOAuthData(null, null, (string)param[0], null, null));
                break;

                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
            }

            return result;
        }

        #endregion

        #region Private

        string GetOAuthData(string Service_name, string host, string consumerKey, string consumerSecret, string accessToken)
        {
            string url = "https://stackexchange.com/oauth/dialog?client_id=2532&scope=no_expiry&redirect_uri=https://stackexchange.com/oauth/login_success";
            if (string.IsNullOrEmpty(Service_name))
            {
                return url;
            }
            else
            {
               return accessToken;
            }
           }

        #endregion

        Uri GetAvatarUri()
        {
            StackOverflowUser user = (StackOverflowUser)((IService)this).VerifyCredential();
            return (user.profile_image != null ? new Uri(user.profile_image) : null);
        }
        
        IReputation GetStackOverflowReputation()
        {
            String str = WebRequestStackOverflow(_host + StackOverflowUri.Default.STACKSCORE);
            str = str.Replace("\"items\":[{", "");
            str = str.Replace("}]", "");

            if (String.IsNullOrEmpty(str)) return null;

            StackOverflowReputation rep = JsonConvert.DeserializeObject<StackOverflowReputation>(str);
            return rep;
        }
        

        #region OAuth2Service
        private String WebRequestStackOverflow(string url)
        {
            url += "&key=" + _consumerKey + "&order=desc&sort=reputation&site=stackoverflow&access_token=" + _accessToken;
            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ServicePoint.Expect100Continue = false;
            var stream = webRequest.GetResponse().GetResponseStream();
            MemoryStream m = new MemoryStream();
            new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress).CopyTo(m);
            try
            {
                string str = Encoding.UTF8.GetString(m.ToArray());
                return str;
            }
            finally
            {
                if (stream != null)
                    ((IDisposable)stream).Dispose();
            }
        }
        #endregion
    }
}
