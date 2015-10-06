using System;
using System.Collections.Generic;
using It.Uniba.Di.Cdg.SocialTfs.OAuthLibrary;
using System.Web;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Facebook
{

    /// <summary>
    /// Rapresent the Facebook social network service.
    /// </summary>
    class FacebookService : OAuth2Service, IService
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal FacebookService()
        {
            _host = null;
            _consumerKey = null;
            _consumerSecret = null;
            _accessToken = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Root path for API of the service.</param>
        /// <param name="consumerKey">Consumer key of the service.</param>
        /// <param name="consumerSecret">Consumer secret of the service.</param>
        /// <param name="accessToken">Access token of the service.</param>
        internal FacebookService(String host, String consumerKey, String consumerSecret, String accessToken)
        {
            _host = host;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
        }

        #endregion

        #region IService

        int IService.Version
        {
            get { return 1; }
        }

        string IService.Name
        {
            get { return "Facebook"; }
        }

        IUser IService.VerifyCredential()
        {
            String jsonUser = WebRequest(_host + FacebookUri.Default.VERIFY_CREDENTIALS);
            return JsonConvert.DeserializeObject<FacebookUser>(jsonUser);
        }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.Avatar);
            features.Add(FeaturesType.UserTimeline);
            features.Add(FeaturesType.Followings);
            features.Add(FeaturesType.Followers);

            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.OAuth2);
            features.Add(FeaturesType.UserTimelineOlderPosts);

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
                case FeaturesType.OAuth2:
                    result = GetOAuthData();
                    break;
                case FeaturesType.Avatar:
                    result = GetAvararUri();
                    break;
                case FeaturesType.UserTimeline:
                    result = GetUserTimeline((long)param[0], (long)param[1], (DateTime)param[2]);
                    break;
                case FeaturesType.UserTimelineOlderPosts:
                    result = GetUserTimelineOlderPosts((long)param[0], (long)param[1], (DateTime)param[2]);
                    break;
                case FeaturesType.Followings:
                case FeaturesType.Followers:
                    result = GetFriend();
                    break;
                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
            }

            return result;
        }

        #endregion

        #region Private

        Uri GetAvararUri()
        {
            return new Uri(_host + FacebookUri.Default.AVATAR + "?access_token=" + _accessToken);
        }

        string[] GetFriend()
        {
            String jsonFriends = WebRequest(_host + FacebookUri.Default.FRIEND);
            FacebookFriendship friends = JsonConvert.DeserializeObject<FacebookFriendship>(jsonFriends);
            List<string> result = new List<string>();
            if(friends != null)
                foreach (IUser item in friends.data)
                    result.Add(item.Id.ToString());
            return result.ToArray();
        }

        IPost[] GetUserTimeline(long id, long sinceId, DateTime sinceDate)
        {
            String jsonPosts;
            if (sinceId == 0)
                jsonPosts = WebRequest(_host + FacebookUri.Default.USER_TIMELINE + "?limit=20");
            else
                jsonPosts = WebRequest(_host + FacebookUri.Default.USER_TIMELINE + "?limit=20&since=" + UnixTimestamp(sinceDate));

            if (jsonPosts == "")
                return new FacebookPost[0];

            //The posts ids are composed in this way: {userid}_{postid} so we remove {userid}
            //jsonPosts = jsonPosts.Replace(String.Concat(id, "_"), "");
            return JsonConvert.DeserializeObject<FacebookTimeline>(jsonPosts).data;
        }

        IPost[] GetUserTimelineOlderPosts(long id, long maxId, DateTime maxDate)
        {
            String jsonPosts = WebRequest(_host + FacebookUri.Default.USER_TIMELINE + "?limit=20&until=" + UnixTimestamp(maxDate - new TimeSpan(0, 0, 1)));
            return JsonConvert.DeserializeObject<FacebookTimeline>(jsonPosts).data;
        }

        String GetOAuthData()
        {
            // fix for issue #1242
            //return "https://www.facebook.com/dialog/oauth?client_id=" + _consumerKey + "&redirect_uri=http://ugres.di.uniba.it:8081/&response_type=token";
            return "https://www.facebook.com/dialog/oauth?client_id=" + _consumerKey + "&redirect_uri=https://socialtfs.codeplex.com/&scope=user_friends%2Cuser_posts%2Cuser_status&response_type=token";
        }
        
        int UnixTimestamp(DateTime datetime)
        {
            return (int)(datetime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        #endregion
    }
}
