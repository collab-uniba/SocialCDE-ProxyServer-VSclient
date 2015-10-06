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
using System.Xml;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Ohloh
{
    class OhlohService : IService
    {
        public string _host;
        public string _username;
        #region Constructors

        internal OhlohService()
        { }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Username on the service.</param>
        /// <param name="password">Password on the service.</param>
        /// <param name="host">Root path of the service.</paparam>
        internal OhlohService(String username, String host)
            
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
            get { return "Ohloh"; }
        }

        IUser IService.VerifyCredential()
        {

            String xmlUser = WebRequestOhloh(_host + OhlohUri.Default.CREDENTIALS + _username + ".xml");
            String jsonUser = xmlToJson(xmlUser);
            jsonUser = jsonUser.Substring(jsonUser.IndexOf("id"));
            jsonUser = "{\"" + jsonUser;
            jsonUser = jsonUser.Remove(jsonUser.Length - 3);
            return JsonConvert.DeserializeObject<OhlohUser>(jsonUser);

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
                    result = GetOhlohReputation();
                    break;
                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
            }

            return result;
        }

        

        
        IReputation GetOhlohReputation()
        {
            String xmlUser = WebRequestOhloh(_host + OhlohUri.Default.CREDENTIALS + _username + ".xml");
            String str = xmlToJson(xmlUser);
            //String str = WebRequestOhloh(_host + "/accounts/" + _username + ".xml");
            //parso prima di endorsements
            str = str.Substring(str.IndexOf("id"));
            
            //rimuovo ultimi 2 char
            str = str.Remove(str.Length - 3);
            String xmlUser2 = WebRequestOhloh(_host + OhlohUri.Default.CREDENTIALS + _username + "/kudos.xml");
            String str2 = xmlToJson(xmlUser2);
            //estrarre solo "items_returned"
            //cancellare tutto ciò che sta prima di "items_returned"
            str2 = str2.Substring(str2.IndexOf("items_returned"));
            //str2 = "{\"" + str2;
            //cancellare tutto ciò che sta dopo la prima virgola
            int index = str2.IndexOf(",");
            str2 = str2.Substring(0, index);
            str2 = str2 + ",\"";
            //accodo "items_returned" in coda a str
            str = str2 + str;
            str = "{\"" + str;
            if (String.IsNullOrEmpty(str)) return null;
            OhlohReputation badges = JsonConvert.DeserializeObject<OhlohReputation>(str);
            OhlohReputation languages = JsonConvert.DeserializeObject<OhlohReputation>(str);
            return JsonConvert.DeserializeObject<OhlohReputation>(str);
        }

        #endregion


        private String WebRequestOhloh(string url)
        {
            url += "?api_key=tG1ac3INF76uh7dH90OOA&v=1";
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

        private string xmlToJson(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            return jsonText;
        }
    }
}
