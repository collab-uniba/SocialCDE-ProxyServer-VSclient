using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub
{
    class GitHubCommit
    {
        public string comments_url { get; set; }
        public string url { get; set; }
        public string sha { get; set; }
        public GitHubUser author { get; set; }
        public GitHubUser committer { get; set; }
        public GitHubSubCommits commit { get; set; } 
    }

    class GitHubSubCommits
    {
        public string url { get; set; }
        public string message { get; set; }
        public int comment_count { get; set; } 
        public struct credential
        {
            public string date { get; set; }
            public string email { get; set; }
            public string name { get; set; } 
        }
    }
}
