using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.TeamFoundationServer
{
    class TeamFoundationServerUser : IUser
    {
        private String _id;
        private String _username;
        private String _domain;

        internal TeamFoundationServerUser(String id, String username, String domain)
        {
            _id = id;
            _username = username;
            _domain = domain;
        }

        string IUser.Id
        {
            get { return _id; }
        }

        string IUser.UserName
        {
            get { return _username; }
        }

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            switch (feature)
            {
                case UserFeaturesType.Domain:
                    return GetDomain();
                default:
                    return null;
            }
        }
        
        string GetDomain()
        {
            return _domain;
        }
    }
}
