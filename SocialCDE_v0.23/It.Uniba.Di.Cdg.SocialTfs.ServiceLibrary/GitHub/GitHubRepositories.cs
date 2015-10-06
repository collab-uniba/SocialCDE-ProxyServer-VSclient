using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub
{
    class GitHubRepositories
    {
        public string url { get; set; }
        public GitHubUser owner { get; set; }
        public string issues_url { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public string open_issues { get; set;  }
        public string git_url { get; set; }
        public string contributors_url { get; set; }
        public string milestones_url { get; set;  }
        public bool has_issues { get; set; }
        public string contents_url { get; set; }
        public string commits_url { get; set; }
        public string collaborators_url { get; set;  }
        
        
    }

    class GitHubIssue
    {
        public string comments_url { get; set; }
        public string url { get; set; }
        public string milestone { get; set; }
        public string state { get; set; }
        public GitHubUser user { get; set; }
        public GitHubUser assignee { get; set; }
        public int comments { get; set; }
        public int number { get; set; }
        public long id { get; set; }
        public string body { get; set; }
        public string title { get; set; }
        public string html_url { get; set; }
        public GitHubLabelFounded[] labels { get; set; }
        
    }

    class GitHubComments
    {
        public string url { get; set; }
        public GitHubUser user { get; set; }
        public long id { get; set; }
        public string body { get; set; } 

    }

    class GitHubLabelFounded
    {
        public string url { get; set; }
        public string name { get; set; }
    }


}
