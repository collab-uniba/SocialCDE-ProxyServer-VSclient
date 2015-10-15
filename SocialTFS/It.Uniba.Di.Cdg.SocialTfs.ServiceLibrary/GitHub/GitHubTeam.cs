using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub
{
    class GitHubTeam
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string description { get; set; }
        public string privacy { get; set; }
        public string permission { get; set; }
        public string members_url { get; set; }
        public string repositories_url { get; set; }
        public int members_count { get; set; }
        public int repos_count { get; set; }
        public Object organization { get; set; }
    }
}
