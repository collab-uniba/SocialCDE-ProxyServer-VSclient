using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Facebook
{
    class FacebookUser : IUser
    {
        #region Attributes

        public long id { get; set; }
        public string username { get; set; }

        #endregion

        string IUser.Id
        {
            get { return this.id.ToString(); }
        }

        string IUser.UserName
        {
            get { return this.username; }
        }

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            throw new NotImplementedException();
        }
    }

    class FacebookFriendship
    {
        public FacebookUser[] data { get; set; }
    }
}
