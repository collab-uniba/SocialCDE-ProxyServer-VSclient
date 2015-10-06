using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;


namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub
{
     class GitHubService : OAuth2Service , IService
    {
        private static Boolean flag = false; 
        

        internal GitHubService()
        {
            _host = null;
            _consumerKey = null;
            _consumerSecret = null;
            _accessToken = null;
            
            
        }

         /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Root path for API of the service.</param>
        /// <param name="consumerKey">Consumer key of the service.</param>
        /// <param name="consumerSecret">Consumer secret of the service.</param>
        /// <param name="accessToken">Access token of the service.</param>
        internal GitHubService(String host, String consumerKey, String consumerSecret, String accessToken)
        {
            _host = host;
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
         
            
           
        }

        #region IService

        int IService.Version
        {
            get { return 1; }
        }

        string IService.Name
        {
            get { return "GitHub"; }
        }

        IUser IService.VerifyCredential()
        {
            
            String jsonUser = WebRequest(_host + "user");
            return JsonConvert.DeserializeObject<GitHubUser>(jsonUser);
            
          
         }

        List<FeaturesType> IService.GetPublicFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.Avatar);
            features.Add(FeaturesType.Followings);
            features.Add(FeaturesType.Followers);
            features.Add(FeaturesType.TFSTeamProject);
            features.Add(FeaturesType.IterationNetwork);
            features.Add(FeaturesType.InteractiveNetwork);
           
            
            return features;
        }

        List<FeaturesType> IService.GetPrivateFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.OAuth2);
            features.Add(FeaturesType.Labels);
            return features;
        }

        List<FeaturesType> IService.GetScoredFeatures()
        {
            List<FeaturesType> features = new List<FeaturesType>();

            features.Add(FeaturesType.Followings);
            features.Add(FeaturesType.Followers);
            features.Add(FeaturesType.TFSTeamProject);
            return features;
        }

        object IService.Get(FeaturesType feature, params object[] param)
        {
            object result = null;

            switch (feature)
            {
                    
                case FeaturesType.OAuth2:
                    if (param.Length > 0)
                    {
                        result = GetOAuthData((string)param[0], (string)param[1], (string)param[2], (string)param[3], (string)param[4]);
                    }
                    else
                    {
                        result = GetOAuthData(null, null, null, null,null);
                    }
                     break;
                case FeaturesType.Avatar:
                    result = GetAvararUri();
                    break;
                case FeaturesType.Followings:
                    result = GetFollowing();
                    break; 
                case FeaturesType.Followers:
                    result = GetFollowers();
                    break;
                    
                case FeaturesType.IterationNetwork:
                    result = GetIterationFriends();
                    break;
                    
                case FeaturesType.InteractiveNetwork:
                    result = GetInteractivePeople();
                    break;
              
              case FeaturesType.TFSTeamProject:
                    result = GetAllPeopleInProject();
                    break;
             
                case FeaturesType.Repository:
                    result = FindRepository((string)param[0]);
                    break; 
                case FeaturesType.Labels:
                    result = RegisterLabels((string) param[0]);
                    break;
                default:
                    throw new NotImplementedException("Use GetAvailableFeatures() to know the implemented methods");
                
             }

            return result;
        }

        private Boolean RegisterLabels(string labels)
        {
            if (!string.IsNullOrEmpty(labels))
            {
                ServiceFactory.GitHubLabels = labels;
                
                return true;
            }
            else
            {
                return false; 
            }
        }

        private int findCharacter(char character,string word)
        {
            Contract.Requires(!String.IsNullOrEmpty(word));
            Contract.Requires(!Char.IsWhiteSpace(character));
            

            char[] sequence = word.ToCharArray();
            int counter = 0;
            flag = false; 
            foreach (char letter in sequence)
            { 
                if(letter.Equals(character))
                {
                    flag = true; 
                }

                if(!flag)
                {
                    counter +=1; 
                }
            }

            return counter; 
        }

        private string[] GetAllPeopleInProject()
        {
            List<string> totalCollaborators = new List<string>();
            System.Diagnostics.Debug.WriteLine("Posizione nulla " + ServiceFactory.GitHubLabels ); 
            String jsonUser = WebRequest(_host + "user");
            GitHubUser currentUser = ( jsonUser != "" ? JsonConvert.DeserializeObject<GitHubUser>(jsonUser) : null);
            String jsonUserRepositories = ( currentUser != null ? WebRequest(currentUser.repos_url) : null);
            GitHubRepositories[] publicRepositories = ( jsonUserRepositories != null ? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonUserRepositories) : new GitHubRepositories[0]);
            String jsonOrganizations = ( currentUser != null ? WebRequest(currentUser.organizations_url) : null);
            GitHubOrganization[] organizations = ( jsonOrganizations != null ? JsonConvert.DeserializeObject<GitHubOrganization[]>(jsonOrganizations) : new GitHubOrganization[0]);

            if (publicRepositories.Length > 0)
            {
                for (int i = 0; i < publicRepositories.Length; i++)
                {
                    string url = (publicRepositories[i].collaborators_url.Contains('{') ? publicRepositories[i].collaborators_url.Substring(0, findCharacter('{', publicRepositories[i].collaborators_url)) : publicRepositories[i].collaborators_url);
                    string jsonCollaborators = WebRequest(url);
                    GitHubUser[] collaborators = ( jsonCollaborators != "" ? JsonConvert.DeserializeObject<GitHubUser[]>(jsonCollaborators) : new GitHubUser[0]);

                    for (int j = 0; j < collaborators.Length; j++)
                    {
                        IUser user = collaborators[j];
                       
                        totalCollaborators.Add(user.Id.ToString());
                    }


                }
            }


            for (int i = 0; i < organizations.Length; i++)
            {
                string url = (organizations[i].repos_url.Contains('{') ? organizations[i].repos_url.Substring(0, findCharacter('{', organizations[i].repos_url)) : organizations[i].repos_url);
                String jsonOrgsRepositories = WebRequest(url);
                GitHubRepositories[] repositories = ( jsonOrgsRepositories != "" ? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonOrgsRepositories) : new GitHubRepositories[0]);


                for (int j = 0; j < repositories.Length; j++)
                {
                    url = (repositories[j].collaborators_url.Contains('{') ? repositories[j].collaborators_url.Substring(0, findCharacter('{', repositories[j].collaborators_url)) : repositories[j].collaborators_url);
                    string jsonCollaborators = WebRequest(url);
                    GitHubUser[] collaborators = (jsonCollaborators != "" ? JsonConvert.DeserializeObject<GitHubUser[]>(jsonCollaborators) : new GitHubUser[0]);

                    for (int h = 0; h < collaborators.Length; h++)
                    {
                        IUser user = collaborators[h];

                        totalCollaborators.Add(user.Id.ToString());
                        
                    }


                }
            }

            for (int i = 0; i < totalCollaborators.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("Collaboratore n. " + i + " id " + totalCollaborators[i].ToString());
            }

            return totalCollaborators.Distinct().ToArray(); 
        }

        string FindRepository(string interactiveObject)
        {
            String jsonUser = WebRequest(_host + "user");
            GitHubUser currentUser = ( jsonUser != "" ?  JsonConvert.DeserializeObject<GitHubUser>(jsonUser) : null);
            String jsonUserRepositories = ( currentUser != null ? WebRequest(currentUser.repos_url) : null) ;
            GitHubRepositories[] publicRepositories = (jsonUserRepositories != null && jsonUserRepositories != ""? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonUserRepositories) : new GitHubRepositories[0]);
            String jsonOrganizations = ( currentUser != null ? WebRequest(currentUser.organizations_url) : null);
            GitHubOrganization[] organizations = ( jsonOrganizations != null ? JsonConvert.DeserializeObject<GitHubOrganization[]>(jsonOrganizations) : new GitHubOrganization[0]);

            String url = String.Empty; 
    

            foreach (GitHubRepositories repos in publicRepositories)
            {
                url = (repos.commits_url.Contains('{') ? repos.commits_url.Substring(0, findCharacter('{', repos.commits_url)) : repos.commits_url);
                String jsonUsersInvolved = WebRequest(url + "?path=" + interactiveObject);
                
                if (!jsonUsersInvolved.Equals("[]") && !String.IsNullOrEmpty(jsonUsersInvolved))
                {
                    return (repos.git_url.Contains('{') ? repos.git_url.Substring(0, findCharacter('{', repos.git_url)) : repos.git_url); 
                }
                
            }

            foreach (GitHubOrganization org in organizations)
            {
                String jsonOrganizationRepositories = WebRequest(org.repos_url);
                GitHubRepositories[] publicOrganizationRepositories = ( jsonOrganizationRepositories != "" ? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonOrganizationRepositories) : new GitHubRepositories[0]);

                foreach (GitHubRepositories orgRepos in publicOrganizationRepositories)
                {
                    url = (orgRepos.commits_url.Contains('{') ? orgRepos.commits_url.Substring(0, findCharacter('{', orgRepos.commits_url)) : orgRepos.commits_url);
                    String jsonUsersInvolved = WebRequest(url + "?path=" + interactiveObject);

                    if (!jsonUsersInvolved.Equals("[]") && !String.IsNullOrEmpty(jsonUsersInvolved))
                    {
                        return (orgRepos.git_url.Contains('{') ? orgRepos.git_url.Substring(0, findCharacter('{', orgRepos.git_url)) : orgRepos.git_url);
                    }
                }
            
            }

            return string.Empty; 
        }

        private SCollection[] GetInteractivePeople()
        {
            
            List<GitHubObject> objects = new List<GitHubObject>();
            Dictionary<GitHubRepositories, List<GitHubObject>> dictionaryFiles = new Dictionary<GitHubRepositories, List<GitHubObject>>();
            
            Contract.Requires(!String.IsNullOrEmpty(_host));
            
            String jsonUser = WebRequest(_host + "user");
            GitHubUser currentUser;
            String jsonUserRepositories;
            GitHubRepositories[] publicRepositories;
            String jsonOrganizations;
            GitHubOrganization[] organizations; 

            currentUser = (jsonUser !=  "" ? JsonConvert.DeserializeObject<GitHubUser>(jsonUser) : null);
            jsonUserRepositories = (currentUser != null ? WebRequest(currentUser.repos_url) : null);
            publicRepositories = ( jsonUserRepositories != null ? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonUserRepositories) : null);
            jsonOrganizations = (currentUser != null? WebRequest(currentUser.organizations_url) : null);
            organizations = (jsonOrganizations != null ? JsonConvert.DeserializeObject<GitHubOrganization[]>(jsonOrganizations) : null);

            if (publicRepositories != null && publicRepositories.Length > 0)
            { 
                 string url = ( publicRepositories[0].contents_url.Contains('{') ? publicRepositories[0].contents_url.Substring(0,findCharacter('{',publicRepositories[0].contents_url)) : publicRepositories[0].contents_url );
                 String jsonRootObjects = WebRequest(url);
                 GitHubObject[] tempObjects = ( jsonRootObjects != "" ? JsonConvert.DeserializeObject<GitHubObject[]>(jsonRootObjects) : null);
                 if (tempObjects.Length > 0)
                 {
                     objects.AddRange(tempObjects);
                 }

                 for (int i = 0; i < objects.Count; i++)
                 {
                     getRepositoriesFiles(ref objects, objects[i],ref i);
                 }

                 dictionaryFiles.Add(publicRepositories[0], objects);

                 if (publicRepositories.Length > 1)
                 {
                     for (int i = 1; i < publicRepositories.Length; i++)
                     {
                         objects = new List<GitHubObject>();

                         url = (publicRepositories[0].contents_url.Contains('{') ? publicRepositories[0].contents_url.Substring(0, findCharacter('{', publicRepositories[0].contents_url)) : publicRepositories[0].contents_url);
                         jsonRootObjects = WebRequest(url);
                         tempObjects = (jsonRootObjects != "" ? JsonConvert.DeserializeObject<GitHubObject[]>(jsonRootObjects) : new GitHubObject[0]);
                         if (tempObjects.Length > 0)
                         {
                             objects.AddRange(tempObjects);
                         }

                         for (int j = 0; j < objects.Count; j++)
                         {
                             getRepositoriesFiles(ref objects, objects[j], ref j);
                         }

                         dictionaryFiles.Add(publicRepositories[i], objects);
                     }
                 }
            }
            if(organizations != null)
            {
                for (int i = 0; i < organizations.Length; i++)
                {
                    string url = (organizations[i].repos_url.Contains('{') ? organizations[i].repos_url.Substring(0, findCharacter('{', organizations[i].repos_url)) : organizations[i].repos_url);
                    String jsonOrgsRepositories = WebRequest(url);
                    GitHubRepositories[] repositories = (jsonOrgsRepositories != null && jsonOrgsRepositories != "" ? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonOrgsRepositories) : null);

                    if(repositories != null)
                    {
                        for (int j = 0; j < repositories.Length; j++)
                        {
                            objects = new List<GitHubObject>();
                            url = (repositories[j].contents_url.Contains('{') ? repositories[j].contents_url.Substring(0, findCharacter('{', repositories[j].contents_url)) : repositories[j].contents_url);
                            String jsonRootObjects = WebRequest(url);
                            GitHubObject[] tempObjects = ( jsonRootObjects != "" ? JsonConvert.DeserializeObject<GitHubObject[]>(jsonRootObjects) : new GitHubObject[0]);
                    
                            if (tempObjects.Length > 0)
                            {
                                objects.AddRange(tempObjects);

                                for (int h = 0; h < objects.Count; h++)
                                {
                                    getRepositoriesFiles(ref objects, objects[h], ref h);
                                }
                            }
                            dictionaryFiles.Add(repositories[j], objects);

                        }
                    }

                }
            }


            List<SCollection> result = new List<SCollection>();

            foreach (KeyValuePair<GitHubRepositories, List<GitHubObject>> pair in dictionaryFiles)
            {
                SCollection collection= new SCollection();
                collection.Uri = pair.Key.git_url; 
                List<SFile> filesFounded = new List<SFile>();
                List<GitHubObject> list = pair.Value;

                for (int i = 0; i < list.Count; i++)
                {
                    SFile fileFounded = new SFile();
                    List<string> usernameUserInvolved = new List<string>();
                    fileFounded.Name = list[i].path ; 
                    String url = (pair.Key.commits_url.Contains('{') ? pair.Key.commits_url.Substring(0, findCharacter('{', pair.Key.commits_url)) : pair.Key.commits_url);
                    String jsonUsersInvolved = WebRequest(url + "?path="+ list[i].path);
                    GitHubCommit[] tempCommit = ( jsonUsersInvolved != null && !String.IsNullOrEmpty(jsonUsersInvolved)  ? JsonConvert.DeserializeObject<GitHubCommit[]>(jsonUsersInvolved) : new GitHubCommit[0]);

                    for (int j = 0; j < tempCommit.Length; j++)
                    {
                        if (tempCommit[j].committer != null && tempCommit[j].author != null)
                        {
                            usernameUserInvolved.Add(tempCommit[j].author.login);
                            usernameUserInvolved.Add(tempCommit[j].committer.login);
                        }
                    }

                    fileFounded.InvolvedUsers = usernameUserInvolved.Distinct().ToArray();

                    filesFounded.Add(fileFounded);
                }

                collection.Files = filesFounded.ToArray();
             
                //taking all repositories from the dictionary
                GitHubRepositories[] repositories = dictionaryFiles.Keys.ToArray();
                List<SWorkItem> issuesFounded = new List<SWorkItem>();
                for (int i = 0; i < repositories.Length; i++)
                {
                    //checking issues in the all repositories
                    if (repositories[i].has_issues)
                    {
                        string issues_url = repositories[i].issues_url;
                        string url = (issues_url.Contains('{') ? issues_url.Substring(0, findCharacter('{', issues_url)) : issues_url);
                        String jsonIssues = WebRequest(url);
                        GitHubIssue[] publicIssues = (jsonIssues != "" ? JsonConvert.DeserializeObject<GitHubIssue[]>(jsonIssues) : null);

                        foreach (GitHubIssue issue in publicIssues)
                        {
                                issuesFounded.Add(new SWorkItem() {
                                Name = issue.title,
                                InvolvedUsers = getUsersIssue(issue)
                            });
                        }
                    }
                }

                collection.WorkItems = issuesFounded.ToArray();  
                result.Add(collection);

                System.Diagnostics.Debug.WriteLine("dizionario " + pair.Key.git_url);
                
            }
          

            return result.ToArray();
        }

        private void getRepositoriesFiles(ref List<GitHubObject> objects, GitHubObject element, ref int index)
        {
            Contract.Ensures(objects != null);
            Contract.Ensures(element != null);


            if (element.type == "dir")
            {
                String jsonObjects = WebRequest(element.url);
                GitHubObject[] tempObjects = ( jsonObjects != "" ? JsonConvert.DeserializeObject<GitHubObject[]>(jsonObjects) : null);
                if (tempObjects != null)
                {
                    objects.AddRange(tempObjects);
                    objects.RemoveAt(index);
                }
                index -= 1; 
            }
         }

        private string[] getUsersIssues(String  issues_url, GitHubUser currentUser)
        {
            List<string> userInWorkItem = new List<string>();

            if (!VerifyFilters())
            {
                string url = (issues_url.Contains('{') ? issues_url.Substring(0, findCharacter('{', issues_url)) : issues_url);
                String jsonIssues = WebRequest(url);
                GitHubIssue[] publicIssues =  ( jsonIssues != "" ? JsonConvert.DeserializeObject<GitHubIssue[]>(jsonIssues) : null);

                if (publicIssues != null && publicIssues.Length > 0)
                {

                    foreach (GitHubIssue issue in publicIssues)
                    {
                       

                            if (AnalyzeIssue(issue, currentUser))
                            {
                                userInWorkItem.AddRange(getUsersIssue(issue, currentUser));
                            }
                        
                    }
                }

            }
            else
            {
                String[] filters = ServiceFactory.GitHubLabels.Split(',');

                string url = (issues_url.Contains('{') ? issues_url.Substring(0, findCharacter('{', issues_url)) : issues_url);
                String jsonIssues = WebRequest(url);
                GitHubIssue[] publicIssues = ( jsonIssues != "" ? JsonConvert.DeserializeObject<GitHubIssue[]>(jsonIssues) : null);

                if (publicIssues != null && publicIssues.Length > 0)
                {

                    foreach (GitHubIssue issue in publicIssues)
                    {
                        Boolean flag = false;

                        for (int i = 0; i < filters.Length; i++)
                        {
                            foreach (GitHubLabelFounded label in issue.labels)
                            {
                                if (label.name.Equals(filters[i].ToString()))
                                {
                                    flag = true;
                                }
                            }
                        }

                        if (AnalyzeIssue(issue, currentUser) && flag)
                        {
                            userInWorkItem.AddRange(getUsersIssue(issue, currentUser));
                        }

                    }
                }


                //scan closed issues

                jsonIssues = WebRequest(url + "?state=closed");
                publicIssues = ( jsonIssues != "" ? JsonConvert.DeserializeObject<GitHubIssue[]>(jsonIssues) : null);

                if (publicIssues != null && publicIssues.Length > 0)
                {

                    foreach (GitHubIssue issue in publicIssues)
                    {
                        Boolean flag = false;

                        for (int i = 0; i < filters.Length; i++)
                        {
                            
                                if ( VerifyLabelContain(filters[i],issue.labels))
                                {
                                    flag = true;
                                }
                            
                        }

                        if (AnalyzeIssue(issue, currentUser) && flag)
                        {
                            userInWorkItem.AddRange(getUsersIssue(issue, currentUser));
                        }

                    }
                }
               

            }
            return userInWorkItem.Distinct().ToArray(); 
        }
        /// <summary>
        /// Check if GitHubSetting label is set correctly
        /// </summary>
        /// <returns>
        /// True if GitHubSetting label is set correctly, false otherwise.
        /// </returns>
        private Boolean VerifyFilters()
        {
            Boolean result = false;
            /*
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load("GitHubSettings.xml");
            XmlNode nodo = myXmlDocument.DocumentElement.ChildNodes[0];*/

            if (!string.IsNullOrEmpty(ServiceFactory.GitHubLabels))
            {
                String[] filters = ServiceFactory.GitHubLabels.Split(',');
                Boolean flag = true;
                foreach (string filter in filters)
                {
                    if(string.IsNullOrEmpty(filter))
                    {
                        flag = false; 
                    }
                    
                }

                 result = flag;
            }
            else
            {
                result = false; 
            }

            return result; 
        }

        private string[] GetIterationFriends()
        {
            Contract.Requires(!String.IsNullOrEmpty(_host));

            List<string> userInWorkItem = new List<string>();
            String jsonUser = WebRequest(_host + "user");
            GitHubUser currentUser;
            String jsonUserRepositories;
            GitHubRepositories[] publicRepositories;
            String jsonOrganizations;
            GitHubOrganization[] organizations;

            currentUser = (jsonUser != null && jsonUser != "" ? JsonConvert.DeserializeObject<GitHubUser>(jsonUser) : null);
            jsonUserRepositories =(currentUser != null?   WebRequest(currentUser.repos_url) : null);
            publicRepositories = (jsonUserRepositories != null && jsonUserRepositories  != "" ? JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonUserRepositories) : null);
            jsonOrganizations = ( currentUser != null ? WebRequest(currentUser.organizations_url) : null);
            organizations = (jsonOrganizations != null && jsonOrganizations  != "" ? JsonConvert.DeserializeObject<GitHubOrganization[]>(jsonOrganizations) : null);

            if (publicRepositories != null)
            {
                foreach (GitHubRepositories repository in publicRepositories)
                {

                    if (repository.has_issues)
                    {
                        userInWorkItem.AddRange(getUsersIssues(repository.issues_url, currentUser));
                    }
                }
            }

            if (organizations != null)
            {
                foreach (GitHubOrganization organization in organizations)
                {

                    string url = (organization.repos_url.Contains('{') ? organization.repos_url.Substring(0, findCharacter('{', organization.repos_url)) : organization.repos_url);
                    String jsonOrgRepositories = WebRequest(url);
                    GitHubRepositories[] repositories = ( jsonOrgRepositories != "" ?  JsonConvert.DeserializeObject<GitHubRepositories[]>(jsonOrgRepositories) : new GitHubRepositories[0]);

                    foreach (GitHubRepositories repository in repositories)
                    {
                        if (repository.has_issues)
                        {
                            userInWorkItem.AddRange(getUsersIssues(repository.issues_url, currentUser));
                        }
                    }
                }
            }

            if (userInWorkItem.Count > 0)
            { 
                userInWorkItem.Add(currentUser.login);
            }

            for (int i = 0; i < userInWorkItem.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine(" numero " + i + " utente " + userInWorkItem[i].ToString());
            }
            
            return userInWorkItem.Distinct().ToArray();
        }

        private Boolean VerifyLabelContain(string label, GitHubLabelFounded[] labels)
        {
            Boolean flag = false;

            foreach (GitHubLabelFounded labelFounded in labels)
            { 
                if(labelFounded.Equals(label))
                {
                    flag = true; 
                }
            }

            return true; 

        }
        /// <summary>
        /// Get all users associated to the issue
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private string[] getUsersIssue(GitHubIssue issue, GitHubUser user)
        {
            Contract.Ensures(issue != null);
            Contract.Ensures(user != null);

            List<string> usersfounded = new List<string>();

            if (!issue.user.id.Equals(user.id))
            {
                usersfounded.Add(issue.user.login);
            }
            else if ( issue.assignee != null &&  !issue.assignee.id.Equals(user.id))
            {
                usersfounded.Add(issue.assignee.login);
            }
            else
            {
                if (issue.comments > 0)
                {
                    string url = (issue.comments_url.Contains('{') ? issue.comments_url.Substring(0, findCharacter('{', issue.comments_url)) : issue.comments_url);
                    String jsonComments = WebRequest(url);
                    GitHubComments[] comments = JsonConvert.DeserializeObject<GitHubComments[]>(jsonComments);


                    foreach (GitHubComments comment in comments)
                    {
                        if (!comment.user.id.Equals(user.id))
                        {
                            usersfounded.Add(comment.user.login);
                        }
                    }
                }
            }

            return usersfounded.Distinct().ToArray();
        }


        /// <summary>
        /// Get all users associated to the issue
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        private string[] getUsersIssue(GitHubIssue issue)
        {
            List<string> usersfounded = new List<string>();
            usersfounded.Add(issue.user.login);

            if (issue.assignee != null)
            {
                usersfounded.Add(issue.assignee.login);
            }

            if (issue.comments > 0)
            {
                string url = (issue.comments_url.Contains('{') ? issue.comments_url.Substring(0, findCharacter('{', issue.comments_url)) : issue.comments_url);
                String jsonComments = WebRequest(url);
                GitHubComments[] comments = JsonConvert.DeserializeObject<GitHubComments[]>(jsonComments);


                foreach (GitHubComments comment in comments)
                {

                    usersfounded.Add(comment.user.login);

                }
            }
            return usersfounded.ToArray();
        }

        /// <summary>
        /// Check if the issue is connected with user.
        /// An issue is connected with a user if it has been created,commented or assigned to the user
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private Boolean AnalyzeIssue(GitHubIssue issue, GitHubUser user)
        {
            Contract.Ensures(issue != null);
            Contract.Ensures(user != null);

            Boolean flag = false; 
            if (issue.user.id.Equals(user.id))
            {
                flag = true; 
            }
            else if (issue.assignee != null && issue.assignee.id.Equals(user.id))
            {
                flag = true;
            }
            else
            {
                if (issue.comments > 0)
                {
                    string url = (issue.comments_url.Contains('{') ? issue.comments_url.Substring(0, findCharacter('{', issue.comments_url)) : issue.comments_url);
                    String jsonComments = WebRequest(url);
                    GitHubComments[] comments = ( jsonComments != ""? JsonConvert.DeserializeObject<GitHubComments[]>(jsonComments) : new GitHubComments[0]);
                    Boolean flag_comments = false; 

                    foreach (GitHubComments comment in comments)
                    {
                        if (comment.user.id.Equals(user.id))
                        {
                            flag_comments = true; 
                        }
                    }

                    if (flag_comments)
                    {
                        flag = true; 
                    }

                }
            }

            return flag;
        }

        Uri GetAvararUri()
        {
            Contract.Requires(!String.IsNullOrEmpty(_host));

            String jsonUser = WebRequest(_host + "user");
            if (!jsonUser.Equals(""))
            {
                return new Uri(JsonConvert.DeserializeObject<GitHubUser>(jsonUser).avatar_url);
            }
            else
            {
                return null; 
            }
          }

        string[] GetFollowers()
        {
            Contract.Requires(!String.IsNullOrEmpty(_host));

            String jsonFriends = WebRequest(_host + "user/followers");
            GitHubUser[]  friends = ( jsonFriends != "" ? JsonConvert.DeserializeObject<GitHubUser[]>(jsonFriends) : new GitHubUser[0]);
            List<string> result = new List<string>();
            foreach (IUser item in friends)
                result.Add(item.Id.ToString());
            return result.ToArray();
        }

        string[] GetFollowing()
        {
            Contract.Requires(!String.IsNullOrEmpty(_host));

            String jsonFriends = WebRequest(_host + "user/following");
            GitHubUser[] friends = ( jsonFriends != "" ? JsonConvert.DeserializeObject<GitHubUser[]>(jsonFriends) : new GitHubUser[0]);
            List<string> result = new List<string>();
            foreach (IUser item in friends)
                result.Add(item.Id.ToString());
            return result.ToArray();
        }

        string   GetOAuthData(string Service_name, string host, string consumerKey, string consumerSecret, string accessToken)
        {
            if (string.IsNullOrEmpty(Service_name))
            {
                return "https://github.com/login/oauth/authorize?client_id=3984a3280445ea55db70";
            }
            else if (!string.IsNullOrEmpty(Service_name) && Service_name.Equals("GitHub"))
            {

                System.Diagnostics.Debug.Print("Prova");
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("client_id", consumerKey);
                param.Add("client_secret", consumerSecret);
                param.Add("code", accessToken);
                String jsonResponse = WebRequestPost("https://github.com/login/oauth/access_token", param);
                String token = JsonConvert.DeserializeObject<GitHubToken>(jsonResponse).access_token;
                return token;
            }
            else
            {
                return string.Empty; 
            }
          

            
        }


        #endregion
    }
}
