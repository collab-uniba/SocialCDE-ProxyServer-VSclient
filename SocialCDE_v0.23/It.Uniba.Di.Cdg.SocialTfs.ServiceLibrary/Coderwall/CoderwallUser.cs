using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Coderwall
{
    public partial class CoderwallUser : IUser
    {

        #region Attributes

        public string username { get; set; }
        public string name { get; set; }
        /*
        public string location { get; set; }
        public int endorsements { get; set; }
        public object team { get; set; }
        public Accounts accounts { get; set; }
        public List<object> badges { get; set; }
         * */

        #endregion

        /*
        public class Accounts
        {
            public object github { get; set; }
        }
         * */

        string IUser.Id
        {
            get { return this.username; }
        }


        String IUser.UserName
        {
            get { return this.username; }
        }


        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            //throw new NotImplementedException();
            return "Coderwall-Domain";
        }
    }
}
