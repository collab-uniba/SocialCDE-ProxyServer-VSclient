using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Yammer
{
    /// <summary>
    /// Rapresent the Yammer microblog.
    /// </summary>
    class YammerService : OAuth1Service, IService
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        internal YammerService()
        {
            _host = null;
            _consumerKey = null;
            _consumerSecret = null;
            _accessToken = null;
            _accessSecret = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="host">Root path for API of the service.</param>
        /// <param name="consumerKey">Consumer key of the service.</param>
        /// <param name="consumerSecret">Consumer secret of the service.</param>
        /// <param name="accessToken">Access token of the service.</param>
        /// <param name="accessSecret">Access secret of the service.</param>
        internal YammerService(String host, String consumerKey, String consumerSecret, String accessToken, String accessSecret)
        {
            _host = host;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessSecret = accessSecret;
        }

        #endregion

        #region IService

        int IService.Version
        {
            get { return 1; }
        }

        String IService.Name
        {
            get { return "YammerService"; }
        }

        IUser IService.VerifyCredential()
        {
            String jsonPosts = GetRequest(_host + YammerUri.Default.VERIFY_CREDENTIALS);
            return JsonConvert.DeserializeObject<YammerUser>(jsonPosts);
        }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.Avatar);
            features.Add(FeaturesType.UserTimeline);

            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.OAuth1);
            features.Add(FeaturesType.UserTimelineOlderPosts);

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
                    result = GetAvararUri();
                    break;
                case FeaturesType.UserTimeline:
                    result = GetUserTimeline((int)param[0], (long)param[1]);
                    break;
                case FeaturesType.UserTimelineOlderPosts:
                    result = GetUserTimelineOlderPosts((int)param[0], (long)param[1]);
                    break;
                case FeaturesType.OAuth1:
                    if ((OAuth1Phase)param[0] == OAuth1Phase.RequestOAuthData)
                        result = GetOAuthData((string)param[1], (string)param[2]);
                    else if ((OAuth1Phase)param[0] == OAuth1Phase.Authorize)
                        result = Authorize((string)param[1], (string)param[2]);
                    break;
                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
            }

            return result;
        }

        #endregion

        #region Private

        IPost[] GetUserTimeline(int id, long sincePost)
        {
            String jsonPosts;
            if (sincePost == 0)
                jsonPosts = GetRequest(_host + YammerUri.Default.USER_TIMELINE);
            else
                jsonPosts = GetRequest(_host + YammerUri.Default.USER_TIMELINE + "?newer_than=" + sincePost);

            return JsonConvert.DeserializeObject<YammerTimeline>(jsonPosts).messages;
        }

        IPost[] GetUserTimelineOlderPosts(int id, long maxId)
        {
            String jsonPosts = GetRequest(_host + YammerUri.Default.USER_TIMELINE + "?older_than=" + maxId);
            return JsonConvert.DeserializeObject<YammerTimeline>(jsonPosts).messages;
        }

        Uri GetAvararUri()
        {
            YammerUser user = (YammerUser)((IService)this).VerifyCredential();
            return new Uri(user.mugshot_url);
        }

        #endregion
    }
}
