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

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Coderwall
{
    class CoderwallService : IService
    {
        public string _host;
        public string _username;
        #region Constructors

        internal CoderwallService()
            
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Username on the service.</param>
        /// <param name="password">Password on the service.</param>
        /// <param name="host">Root path of the service.</paparam>
        internal CoderwallService(String username, String host)
            
        {
            _host = host;
            _username = username;
            //_consumerKey = consumerKey;
            //_consumerSecret = consumerSecret;
            //_accessToken = accessToken;
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
            get { return "Coderwall"; }
        }

        IUser IService.VerifyCredential()
        {

            String jsonUser = WebRequestCoderwall(_host + _username + ".json");
            return JsonConvert.DeserializeObject<CoderwallUser>(jsonUser);
        }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();
            //features.Add(FeaturesType.Avatar);
            features.Add(FeaturesType.Reputation);
            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();
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
                case FeaturesType.Reputation:
                    result = GetCoderwallReputation();
                    break;
                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
            }

            return result;
        }

        #endregion

        IReputation GetCoderwallReputation()
        {
            String str = WebRequestCoderwall(_host + _username + ".json");
            //parso prima di endorsements
            str = str.Substring(str.IndexOf("endorsements"));
            str = "{\"" + str;
            //parso dopo team
            int index = str.IndexOf("team");
            str = str.Substring(0, index);
            //rimuovo ultimi 2 char
            str = str.Remove(str.Length - 2);
            //accodo parentesi
            str = str + "}";
            if (String.IsNullOrEmpty(str)) return null;
            return JsonConvert.DeserializeObject<CoderwallReputation>(str);
        }

        private String WebRequestCoderwall(string url)
        {
            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ServicePoint.Expect100Continue = false;
            try
            {
                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                    return responseReader.ReadToEnd();
            }
            catch
            {
                return String.Empty;
            }
        }
    }
}
