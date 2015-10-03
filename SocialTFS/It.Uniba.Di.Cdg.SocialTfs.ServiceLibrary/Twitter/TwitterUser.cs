using System;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Twitter
{
    /// <summary>
    /// Rapresent a user in Twitter
    /// </summary>
    /// <remarks>
    /// All public attributes are matched by the JSON web response.
    /// In this way it's possible to serizalize automatically from the web response.
    /// </remarks>
    class TwitterUser : IUser
    {
        #region Attributes

        public int id { get; set; }
        public string screen_name { get; set; }
        public string profile_image_url { get; set; }

        #endregion

        String IUser.Id
        {
            get { return this.id.ToString(); }
        }

        String IUser.UserName
        {
            get { return this.screen_name; }
        }

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            throw new NotImplementedException();
        }
    }

    class TwitterFriend
    {
        public string[] ids { get; set; }
    }
}
