using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.StackOverflow
{
    public partial class StackOverflowUser : IUser
    {
        #region Attributes

        public string account_id { get; set; }
        public string profile_image { get; set; }

        #endregion


        string IUser.Id
        {
            get { return this.account_id; }
        }

        
        String IUser.UserName
        {
            get { return "No univoque name on StackOverflow!"; }
        }
        

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            throw new NotImplementedException();
        }
    }
}
