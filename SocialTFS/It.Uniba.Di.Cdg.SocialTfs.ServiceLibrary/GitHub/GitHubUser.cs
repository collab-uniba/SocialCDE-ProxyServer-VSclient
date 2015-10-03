using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub
{
    class GitHubUser : IUser
    {
        public string events_url  { get; set; }
        public string followers_url { get; set; }
        //public string created_at { get; set;  }
        public string following_url { get; set; }
       // public string public_repos { get; set; }
        public string repos_url { get; set; }
        public string login { get; set; }
        //public string followers { get; set; }
        public string id { get; set; }
        //public string following { get; set; }
        public string organizations_url { get; set; }
        public string avatar_url { get; set; }



          string IUser.Id
        {
            get { return this.id.ToString(); }
        }

        string IUser.UserName
        {
            get{ return this.login.ToString();  }
        }

        object IUser.Get(UserFeaturesType feature, params object[] param)
        {
            throw new NotImplementedException();
        }
    }

    

 }

