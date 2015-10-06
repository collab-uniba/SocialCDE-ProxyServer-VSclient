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

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Linkedin
{
    /// <summary>
    /// Rapresent the LinkedIn social network service.
    /// </summary>
    
    class LinkedInService : OAuth2Service, IService
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal LinkedInService()
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
       // internal LinkedInService(String host, String consumerKey, String consumerSecret, String accessToken, String accessSecret)
        internal LinkedInService(String host, String consumerKey, String consumerSecret, String accessToken)
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
            get { return "LinkedIn"; }
        }
        
        IUser IService.VerifyCredential()
        {
            
            String jsonUser = WebRequestLinkedIn(_host + LinkedInUri.Default.VERIFY_CREDENTIALS);
            return JsonConvert.DeserializeObject<LinkedInUser>(jsonUser);
           
        }
        
        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.Avatar);
            features.Add(FeaturesType.Skills);
            features.Add(FeaturesType.Followings);
            features.Add(FeaturesType.Followers);

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

            features.Add(FeaturesType.Followings);
            features.Add(FeaturesType.Followers);

            return features;
        }

        object IService.Get(FeaturesType feature, params object[] param)
        {
            object result = null;

            switch (feature)
            {
                case FeaturesType.Avatar:
                    result = GetAvararUri();
                    break;
                case FeaturesType.Skills:
                    result = GetSkills();
                    break;
                    
                case FeaturesType.Followings:
                    result = GetFriend();
                    break;
                case FeaturesType.Followers:
                    result = GetFriend();
                    break;
                  
                case FeaturesType.OAuth2:
                    result = ( param.Length > 1 ?  GetOAuthData((string)param[0], (string)param[1], (string)param[2], (string)param[3], (string)param[4]) : GetOAuthData(null,null,(string)param[0],null,null));
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


            if (string.IsNullOrEmpty(Service_name))
            {
                //return "https://www.linkedin.com/uas/oauth2/authorization?response_type=code&client_id=" + consumerKey + "&scope=r_fullprofile%20r_network&state=collab2013&redirect_uri=https://socialtfs.codeplex.com";

                return "https://www.linkedin.com/uas/oauth2/authorization?response_type=code&client_id=" + consumerKey + "&scope=r_basicprofile&state=collab2013&redirect_uri=https://socialtfs.codeplex.com";
            }
            else
            {
                String jsonResponse = WebRequestPost("https://www.linkedin.com/uas/oauth2/accessToken?grant_type=authorization_code&code=" + accessToken + "&redirect_uri=https://socialtfs.codeplex.com&client_id=" + consumerKey + "&client_secret=" + consumerSecret, new Dictionary<string, string>());
                String token = (jsonResponse != "" ? JsonConvert.DeserializeObject<LinkedInToken>(jsonResponse).access_token : string.Empty);
                return token; 
            }

            
        }
         string[] GetSkills()
        {
            String jsonSkills = WebRequestLinkedIn(_host + LinkedInUri.Default.SKILLS);
            LinkedInSkills skills = (jsonSkills != null || jsonSkills != "" ? JsonConvert.DeserializeObject<LinkedInSkills>(jsonSkills) : null);
            if (skills == null)
            {
                return new String[0];
            }
            else
            {
                return skills.GetSkillsName();
            }
        }
        
        string[] GetFriend()
        {
            String jsonFriends = WebRequestLinkedIn(_host + LinkedInUri.Default.FRIENDS);
            LinkedInFriendship friends = ( jsonFriends != "" ? JsonConvert.DeserializeObject<LinkedInFriendship>(jsonFriends) : new LinkedInFriendship() );

            friends.values = (friends.values == null ? new LinkedInUser[0] : friends.values);
            
            List<string> result = new List<string>();
            try
            {
                foreach (IUser item in friends.values)
                    result.Add(item.Id.ToString());
                return result.ToArray();
            }
            catch 
            {
                return new List<string>().ToArray();
            }
          
            
        }
        
        Uri GetAvararUri()
        {
            LinkedInUser user = (LinkedInUser)((IService)this).VerifyCredential();
            
            return (user.pictureUrl != null ? new Uri(user.pictureUrl) : null);
        }
        
        #endregion

        #region OAuth2Service
        private  String WebRequestLinkedIn(string url)
        {
            url += (String.IsNullOrEmpty(new Uri(url).Query) ? "?" : "&") + "oauth2_access_token=" + _accessToken;
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

        #endregion 
    }
}
