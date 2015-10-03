using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Ohloh
{
    public partial class OhlohUser : IUser
    {
        #region Attributes

        public string id { get; set; }
        public string name { get; set; }

        #endregion


        string IUser.Id
        {
            get { return this.id; }
        }


        String IUser.UserName
        {
            get { return this.name; }
        }


        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            return "Ohloh-Domain";
        }
    }
}
