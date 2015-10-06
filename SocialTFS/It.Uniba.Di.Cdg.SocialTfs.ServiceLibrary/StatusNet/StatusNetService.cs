using System;
using System.Diagnostics.Contracts;
using It.Uniba.Di.Cdg.SocialTfs.OAuthLibrary;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.StatusNet
{
    /// <summary>
    /// Rapresent the StatusNet microblog.
    /// </summary>
    class StatusNetService : OAuth1Service, IService
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal StatusNetService()
        {
            _host = null;
            _consumerKey = null;
            _consumerSecret = null;
            _accessToken = null;
            _accessSecret = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Root path for API of the service.</param>
        /// <param name="consumerKey">Consumer key of the service.</param>
        /// <param name="consumerSecret">Consumer secret of the service.</param>
        /// <param name="accessToken">Access token of the service.</param>
        /// <param name="accessSecret">Access secret of the service.</param>
        internal StatusNetService(String host, String consumerKey, String consumerSecret, String accessToken, String accessSecret)
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
            get { return "StatusNet"; }
        }

        IUser IService.VerifyCredential()
        {
            String jsonUser = GetRequest(_host + StatusNetUri.Default.VERIFY_CREDENTIALS);
            return JsonConvert.DeserializeObject<StatusNetUser>(jsonUser);
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

            features.Add(FeaturesType.OAuth1);
            features.Add(FeaturesType.MoreInstance);
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
                case FeaturesType.Avatar:
                    result = GetAvararUri();
                    break;
                case FeaturesType.UserTimeline:
                    result = GetUserTimeline((int)param[0], (long)param[1]);
                    break;
                case FeaturesType.UserTimelineOlderPosts:
                    result = GetUserTimelineOlderPosts((int)param[0], (long)param[1]);
                    break;
                case FeaturesType.Followings:
                    result = GetFollowings();
                    break;
                case FeaturesType.Followers:
                    result = GetFollowers();
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
            String jsonPosts = GetRequest(_host + StatusNetUri.Default.USER_TIMELINE + "?id=" + id + "&since_id=" + sincePost);
            IPost[] posts = JsonConvert.DeserializeObject<StatusNetPost[]>(jsonPosts);
            if (posts == null)
                return new StatusNetPost[0];
            else
                return posts;
        }

        IPost[] GetUserTimelineOlderPosts(int id, long maxId)
        {
            String jsonPosts = GetRequest(_host + StatusNetUri.Default.USER_TIMELINE + "?id=" + id + "&max_id=" + (maxId - 1));
            IPost[] posts = JsonConvert.DeserializeObject<StatusNetPost[]>(jsonPosts);
            if (posts == null)
                return new StatusNetPost[0];
            else
                return posts;
        }

        string[] GetFollowings()
        {
            String jsonFollowings = GetRequest(_host + StatusNetUri.Default.FOLLOWINGS);
            return JsonConvert.DeserializeObject<string[]>(jsonFollowings);
        }

        string[] GetFollowers()
        {
            String jsonFollowers = GetRequest(_host + StatusNetUri.Default.FOLLOWERS);
            return JsonConvert.DeserializeObject<string[]>(jsonFollowers);
        }

        Uri GetAvararUri()
        {
            StatusNetUser user = (StatusNetUser)((IService)this).VerifyCredential();
            return new Uri(user.profile_image_url);
        }

        #endregion
    }
}
