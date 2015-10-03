using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Yammer
{
    class YammerUser : IUser
    {
        #region Attributes

        public int id { get; set; }
        public string name { get; set; }
        public string mugshot_url { get; set; }

        #endregion

        string IUser.Id
        {
            get { return id.ToString(); }
        }

        string IUser.UserName
        {
            get { return name; }
        }

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            throw new NotImplementedException();
        }
    }
}
