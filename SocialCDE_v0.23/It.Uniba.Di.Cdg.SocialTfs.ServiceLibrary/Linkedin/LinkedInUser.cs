using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Linkedin
{
    public partial class LinkedInUser : IUser
    {
        #region Attributes

        public string id { get; set; }
        public string pictureUrl { get; set; }

        #endregion

        string IUser.Id
        {
            get { return this.id; }
        }

        String IUser.UserName
        {
            get { return "No univoque name on LinkedIn!"; }
        }

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            throw new NotImplementedException();
        }
    }

    public class LinkedInFriendship
    {
        public LinkedInUser[] values { get; set; }
    }
}
