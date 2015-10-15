using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub;
using log4net.Config;
using log4net;


namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer
{
    enum HiddenType
    {
        Suggestions,
        Dynamic,
        Interactive
    }

    /// <summary>
    /// Implements of all functions of the Proxy web service.
    /// </summary>
    /// <remarks>
    /// Lists all methods available on the web via REST requests
    /// to query or modify the services database.
    /// The complete list of methods' description is in It.Uniba.Di.Cdg.SocialTfs.SharedLibrary.ISocialTFSProxy.cs.
    /// </remarks>
    public class SocialTFSProxy : ISocialTFSProxy
    {
        private int postLimit = 20;
        private TimeSpan _postSpan = new TimeSpan(0, 1, 0);
        private TimeSpan _suggestionSpan = new TimeSpan(24, 0, 0);
        private TimeSpan _dynamicSpan = new TimeSpan(6, 0, 0);
        private TimeSpan _interactiveSpan = new TimeSpan(6, 0, 0);
        private TimeSpan _skillSpan = new TimeSpan(15, 0, 0, 0);

        /// <summary>
        /// This static constructor is called only one time, when the application is started. 
        /// It synchronizes the features available for each service with the features available in the database.
        /// </summary>
        static SocialTFSProxy()
        {
            ConnectorDataContext db = new ConnectorDataContext();
            XmlConfigurator.Configure(new Uri(System.Web.Hosting.HostingEnvironment.MapPath("~/log4net.config")));

            //add the completely new features
            IEnumerable<FeaturesType> features = FeaturesManager.GetFeatures();
            foreach (FeaturesType featureType in features)
            {
                if (!db.Features.Contains(new Feature() { name = featureType.ToString() }))
                {
                    db.Features.InsertOnSubmit(new Feature()
                    {
                        name = featureType.ToString(),
                        description = FeaturesManager.GetFeatureDescription(featureType),
                        @public = FeaturesManager.IsPublicFeature(featureType)
                    });
                }
            }
            db.SubmitChanges();
        }

        public bool IsWebServiceRunning()
        {
            return true;
        }

        public bool IsAvailable(String username)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));

            ConnectorDataContext db = new ConnectorDataContext();

            try
            {
                User user = db.Users.Where(u => u.username == username && u.active).Single();
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        public int SubscribeUser(String email, String password, String username)
        {
            Contract.Requires(!String.IsNullOrEmpty(email));
            Contract.Requires(!String.IsNullOrEmpty(password));
            Contract.Requires(!String.IsNullOrEmpty(username));

            ConnectorDataContext db = new ConnectorDataContext();
            User user;
            try
            {
                user = db.Users.Where(u => u.email == email).Single();
            }
            catch (InvalidOperationException)
            {
                return 1;
            }

            if (user.password != db.Encrypt(password))
                return 2;

            if (!IsAvailable(username))
                return 3;

            user.username = username;
            user.active = true;

            Registration registration = new Registration()
            {
                User = user,
                serviceInstance = db.ServiceInstances.Where(si => si.Service.name == "SocialTFS").Single().id,
                nameOnService = username,
                idOnService = username
            };
            db.Registrations.InsertOnSubmit(registration);
            db.SubmitChanges();

            db.ChosenFeatures.InsertOnSubmit(new ChosenFeature()
            {
                Registration = registration,
                feature = FeaturesType.Post.ToString(),
                lastDownload = new DateTime(1900, 1, 1)
            });
            db.SubmitChanges();

            return 0;
        }

        public bool ChangePassword(String username, String oldPassword, String newPassword)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(oldPassword));
            Contract.Requires(!String.IsNullOrEmpty(newPassword));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, oldPassword);
            if (user == null)
                return false;

            user.password = db.Encrypt(newPassword);
            db.SubmitChanges();
            return true;
        }

        public WService[] GetServices(String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WService[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",S");
            List<WService> result = new List<WService>();

            foreach (ServiceInstance item in db.ServiceInstances.Where(si => si.Service.name != "SocialTFS"))
            {
                result.Add(Converter.ServiceInstanceToWService(db, user, item, true));
            }
            return result.ToArray();
        }

        public WUser GetUser(String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);

            if (user == null)
                return null;

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",U");
            return Converter.UserToWUser(db, user, user, true);
        }

        public WUser GetColleagueProfile(String username, String password, int colleagueId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);

            if (user == null)
                return null;

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",C");
            User colleague = null;
            try
            {
                colleague = db.Users.Where(u => u.id == colleagueId).Single();
            }
            catch
            {
                return null;
            }

            return Converter.UserToWUser(db, user, colleague, true);
        }

        public WOAuthData GetOAuthData(string username, string password, int service)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return null;

            ServiceInstance si = db.ServiceInstances.Where(s => s.id == service).Single();

            IService iService = ServiceFactory.getServiceOauth(si.Service.name, si.host, si.consumerKey, si.consumerSecret, null, null);
            if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth1))
            {
                OAuthAccessData oauthData = iService.Get(FeaturesType.OAuth1, OAuth1Phase.RequestOAuthData, si.host + si.Service.requestToken, si.host + si.Service.authorize) as OAuthAccessData;
                return new WOAuthData()
                {
                    AuthorizationLink = oauthData.RequestUri,
                    AccessToken = oauthData.AccessToken,
                    AccessSecret = oauthData.AccessSecret
                };
            }
            else if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth2))
            {
                String authorizationLink = String.Empty;

                if (si.Service.name.Equals("LinkedIn"))
                {
                    authorizationLink = iService.Get(FeaturesType.OAuth2, si.consumerKey) as String;
                }
                else
                {
                    authorizationLink = iService.Get(FeaturesType.OAuth2) as String;
                }

                return new WOAuthData()
                {
                    AuthorizationLink = authorizationLink
                };
            }
            return null;
        }




        public bool Authorize(string username, string password, int service, string verifier, string accessToken, string accessSecret)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));
            Contract.Requires(!String.IsNullOrEmpty(accessToken));



            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            ServiceInstance si = db.ServiceInstances.Where(s => s.id == service).Single();
            IService iService = ServiceFactory.getService(si.Service.name);



            if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth1))
            {

                iService = ServiceFactory.getServiceOauth(si.Service.name, si.host, si.consumerKey, si.consumerSecret, accessToken, accessSecret);

                OAuthAccessData oauthData = iService.Get(FeaturesType.OAuth1, OAuth1Phase.Authorize, si.host + si.Service.accessToken, verifier) as OAuthAccessData;

                if (oauthData == null)
                    return false;

                IUser iUser = iService.VerifyCredential();

                return RegisterUserOnAService(db, user, si, iUser, oauthData.AccessToken, oauthData.AccessSecret);

            }
            else if (iService.GetPrivateFeatures().Contains(FeaturesType.OAuth2))
            {


                if (si.Service.name.Equals("GitHub") || si.Service.name.Equals("LinkedIn"))
                {
                    accessToken = iService.Get(FeaturesType.OAuth2, si.Service.name, si.host, si.consumerKey, si.consumerSecret, accessToken) as string;
                }

                iService = ServiceFactory.getServiceOauth(si.Service.name, si.host, si.consumerKey, si.consumerSecret, accessToken, null);

                IUser iUser = iService.VerifyCredential();



                return RegisterUserOnAService(db, user, si, iUser, accessToken, null);

            }
            return false;
        }

        public bool RecordService(string username, string password, int service, string usernameOnService, string passwordOnService, string domain)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));
            Contract.Requires(!String.IsNullOrEmpty(usernameOnService));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            ServiceInstance serviceInstance = db.ServiceInstances.Where(s => s.id == service).Single();

            IService iService = ServiceFactory.getService(
                serviceInstance.Service.name,
                usernameOnService,
                passwordOnService,
                domain,
                serviceInstance.host);

            IUser iUser = iService.VerifyCredential();

            return RegisterUserOnAService(db, user, serviceInstance, iUser, db.EncDecRc4("key", passwordOnService), (String)iUser.Get(UserFeaturesType.Domain));
        }

        private bool RegisterUserOnAService(ConnectorDataContext db, User user, ServiceInstance serviceInstance, IUser iUser, String accessToken, String accessSecret)
        {
            try
            {
                Registration reg = new Registration();
                
                reg.user = user.id;
                reg.ServiceInstance = serviceInstance;
                reg.nameOnService = iUser.UserName != null? iUser.UserName : iUser.Id;
                reg.idOnService = iUser.Id.ToString();
                reg.accessToken = accessToken;
                reg.accessSecret = accessSecret;
                
                
                db.Registrations.InsertOnSubmit(reg);

                db.SubmitChanges();
                return true;
            }
            catch (ChangeConflictException)
            {
                return false;
            }
        }

        public bool DeleteRegistredService(String username, String password, int service)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                db.Registrations.DeleteAllOnSubmit(db.Registrations.Where(r => r.user == user.id && r.serviceInstance == service));
                db.SubmitChanges();
            }
            catch (ChangeConflictException)
            {
                return false;
            }
            return true;
        }

        public WPost[] GetHomeTimeline(string username, string password, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",HT");

            List<int> authors = db.StaticFriends.Where(f => f.user == user.id).Select(f => f.friend).ToList();
            authors.Add(user.id);

            return GetTimeline(db, user, authors, since, to);
        }

        public WPost[] GetUserTimeline(string username, string password, string ownerName, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",UT");
            List<int> authors = new List<int> { db.Users.Where(u => u.username == ownerName).Single().id };

            return GetTimeline(db, user, authors, since, to);
        }


        public WPost[] GetIterationTimeline(string username, string password, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",IT");

            List<int> hiddenAuthors = db.Hiddens.Where(h => h.user == user.id && h.timeline == HiddenType.Dynamic.ToString()).Select(h => h.friend).ToList();
            List<int> authors = db.DynamicFriends.Where(f => f.ChosenFeature.user == user.id && !hiddenAuthors.Contains(f.user)).Select(f => f.user).ToList();
            WPost[] timeline = GetTimeline(db, user, authors, since, to);

            new Thread(thread => UpdateDynamicFriend(user)).Start();

            return timeline;
        }

        private String GetFriendString(int userId, Dictionary<int, HashSet<int>> friends)
        {
            String friendsString = "";
            foreach (KeyValuePair<int, HashSet<int>> friend in friends)
            {
                if (friend.Key == userId)
                    continue;

                friendsString += friend.Key + "[";
                String servicesString = "";
                foreach (int service in friend.Value)
                {
                    servicesString += service + ";";
                }
                if (servicesString.Length > 0)
                    servicesString = servicesString.Substring(0, servicesString.Length - 1);
                friendsString += servicesString + "];";
            }
            if (friendsString.Length > 0)
                friendsString = friendsString.Substring(0, friendsString.Length - 1);
            return friendsString;
        }

        private void UpdateDynamicFriend(User user)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            Dictionary<int, HashSet<int>> logFriends = new Dictionary<int, HashSet<int>>();
            bool needToLog = false;

            foreach (ChosenFeature chosenFeature in db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.IterationNetwork.ToString()))
            {
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                if (temp.lastDownload > DateTime.UtcNow - _dynamicSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try
                {
                    db.SubmitChanges();
                    needToLog = true;
                }
                catch
                {
                    continue;
                }

                IService service;
                //submit new friendship for the current chosen feature
                if (temp.Registration.ServiceInstance.Service.name.Equals("GitHub"))
                {
                    service = ServiceFactory.getServiceOauth(temp.Registration.ServiceInstance.Service.name, temp.Registration.ServiceInstance.host, temp.Registration.ServiceInstance.consumerKey, temp.Registration.ServiceInstance.consumerSecret, temp.Registration.accessToken, null);
                }
                else
                {
                    service = ServiceFactory.getService(
                        temp.Registration.ServiceInstance.Service.name,
                        temp.Registration.nameOnService,
                        db.EncDecRc4("key", temp.Registration.accessToken),
                        temp.Registration.accessSecret,
                        temp.Registration.ServiceInstance.host);
                }
                //this line must be before the deleting
                String[] dynamicFriends = (String[])service.Get(FeaturesType.IterationNetwork, null);

                //delete old friendship for the current chosen feature
                db.DynamicFriends.DeleteAllOnSubmit(db.DynamicFriends.Where(df => df.chosenFeature == temp.id));
                db.SubmitChanges();

                foreach (String dynamicFriend in dynamicFriends)
                {
                    IEnumerable<int> friendsInDb = db.Registrations.Where(r => r.nameOnService == dynamicFriend && r.serviceInstance == temp.serviceInstance).Select(r => r.user);
                    foreach (int friendInDb in friendsInDb)
                    {
                        db.DynamicFriends.InsertOnSubmit(new DynamicFriend()
                        {
                            chosenFeature = temp.id,
                            user = friendInDb
                        });

                        if (!logFriends.ContainsKey(friendInDb))
                            logFriends[friendInDb] = new HashSet<int>();
                        logFriends[friendInDb].Add(temp.Registration.serviceInstance);
                    }
                }
                try
                {
                    db.SubmitChanges();
                }
                catch { }
            }

            if (needToLog)
            {
                ILog log = LogManager.GetLogger("NetworkLogger");
                log.Info(user.id + ",I,[" + GetFriendString(user.id, logFriends) + "]");
            }
        }

        public WPost[] GetInteractiveTimeline(string username, string password, string collectionUri, string interactiveObject, string objectType, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",JT");

            if (String.IsNullOrEmpty(collectionUri))
            {
                collectionUri = FindGithubRepository(user, interactiveObject);

            } else if (collectionUri.Contains("git://")) {  //in this case the interactive object is an github issue
                GetUsersIssuesInvolved(user, collectionUri, interactiveObject);
            }


            List<int> hiddenAuthors = db.Hiddens.Where(h => h.user == user.id && h.timeline == HiddenType.Interactive.ToString()).Select(h => h.friend).ToList();
            List<int> authors = db.InteractiveFriends.Where(f => /*f.ChosenFeature.user == user.id &&*/ f.collection == collectionUri && f.interactiveObject.EndsWith(interactiveObject) && f.objectType == objectType && !hiddenAuthors.Contains(f.user)).Select(f => f.user).ToList();
            WPost[] timeline = GetTimeline(db, user, authors, since, to);

            new Thread(thread => UpdateInteractiveFriend(user)).Start();

            log = LogManager.GetLogger("NetworkLogger");
            String authorsString = "";
            foreach (int author in authors)
            {
                if (author == user.id)
                    continue;
                authorsString += author + ";";
            }
            if (authorsString.Length > 0)
                authorsString = authorsString.Substring(0, authorsString.Length - 1);

            log.Info(user.id + ",J,[" + authorsString + "]," + collectionUri + "," + objectType + "," + interactiveObject);

            return timeline;
        }

        private void GetUsersIssuesInvolved(User user, string collectionUri, string issueId)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            Boolean flag = false;
            IService service = null;
            ChosenFeature temp = null;
            foreach (ChosenFeature chosenFeature in db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.InteractiveNetwork.ToString()))
            {
                temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();

                if (temp.Registration.ServiceInstance.Service.name.Equals("GitHub"))
                {
                    service = ServiceFactory.getServiceOauth(temp.Registration.ServiceInstance.Service.name, temp.Registration.ServiceInstance.host, temp.Registration.ServiceInstance.consumerKey, temp.Registration.ServiceInstance.consumerSecret, temp.Registration.accessToken, null);
                    flag = true;
                }
            }

            if (flag)
            {
                //obtaining users involved in the issue
                String[] users = (String[])service.Get(FeaturesType.UsersIssuesInvolved, new Object[2] { collectionUri, issueId });
                 
                SWorkItem workitem = new SWorkItem()
                        {
                            Name = issueId,
                            InvolvedUsers = users
                        };

                
                db.InteractiveFriends.DeleteAllOnSubmit(db.InteractiveFriends.Where(intFr => intFr.chosenFeature == temp.id && intFr.objectType == "WorkItem"));
                db.SubmitChanges();

                IEnumerable<int> friendsInDb = db.Registrations.Where(r => workitem.InvolvedUsers.Contains(r.nameOnService) || workitem.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.user);
                foreach (int friendInDb in friendsInDb)
                {
                    db.InteractiveFriends.InsertOnSubmit(new InteractiveFriend()
                    {
                        user = friendInDb,
                        chosenFeature = temp.id,
                        collection = collectionUri,
                        interactiveObject = workitem.Name,
                        objectType = "WorkItem"
                    });
                }

                db.SubmitChanges();
            }
          
        }

        private string FindGithubRepository(User user, string interactiveObject)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            Boolean flag = false;
            IService service = null;

            foreach (ChosenFeature chosenFeature in db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.InteractiveNetwork.ToString()))
            {
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();

                if (temp.Registration.ServiceInstance.Service.name.Equals("GitHub"))
                {
                    service = ServiceFactory.getServiceOauth(temp.Registration.ServiceInstance.Service.name, temp.Registration.ServiceInstance.host, temp.Registration.ServiceInstance.consumerKey, temp.Registration.ServiceInstance.consumerSecret, temp.Registration.accessToken, null);
                    flag = true;
                }
            }

            if (flag)
            {
                return (String)service.Get(FeaturesType.Repository, new Object[1] { interactiveObject });

            }
            else
            {
                return string.Empty;
            }
        }

        private void UpdateInteractiveFriend(User user)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            foreach (ChosenFeature chosenFeature in db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.InteractiveNetwork.ToString()))
            {
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                if (temp.lastDownload > DateTime.UtcNow - _interactiveSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try { db.SubmitChanges(); }
                catch { continue; }

                IService service;
                if (temp.Registration.ServiceInstance.Service.name.Equals("GitHub"))
                {
                    service = ServiceFactory.getServiceOauth(temp.Registration.ServiceInstance.Service.name, temp.Registration.ServiceInstance.host, temp.Registration.ServiceInstance.consumerKey, temp.Registration.ServiceInstance.consumerSecret, temp.Registration.accessToken, null);
                }
                else
                {
                    //submit new friendship for the current chosen feature
                    service = ServiceFactory.getService(
                       temp.Registration.ServiceInstance.Service.name,
                       temp.Registration.nameOnService,
                       db.EncDecRc4("key", temp.Registration.accessToken),
                       temp.Registration.accessSecret,
                       temp.Registration.ServiceInstance.host);
                }
                //this line must be before the deleting
                SCollection[] collections = (SCollection[])service.Get(FeaturesType.InteractiveNetwork);
                try
                {
                    //vecchio metodo
                    db.InteractiveFriends.DeleteAllOnSubmit(db.InteractiveFriends.Where(intFr => intFr.chosenFeature == temp.id)); 
                    db.SubmitChanges();

                    foreach (SCollection collection in collections)
                    {
                        foreach (SWorkItem workitem in collection.WorkItems)
                        {
                            IEnumerable<int> friendsInDb = db.Registrations.Where(r => workitem.InvolvedUsers.Contains(r.nameOnService) || workitem.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.user);
                            foreach (int friendInDb in friendsInDb)
                            {
                                db.InteractiveFriends.InsertOnSubmit(new InteractiveFriend()
                                {
                                    user = friendInDb,
                                    chosenFeature = temp.id,
                                    collection = collection.Uri,
                                    interactiveObject = workitem.Name,
                                    objectType = "WorkItem"
                                });
                            }
                        }
                        foreach (SFile file in collection.Files)
                        {
                            IEnumerable<int> friendsInDb = db.Registrations.Where(r => file.InvolvedUsers.Contains(r.nameOnService) || file.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.user);
                            foreach (int friendInDb in friendsInDb)
                            {
                                db.InteractiveFriends.InsertOnSubmit(new InteractiveFriend()
                                {
                                    user = friendInDb,
                                    chosenFeature = temp.id,
                                    collection = collection.Uri,
                                    interactiveObject = file.Name,
                                    objectType = "File"
                                });
                            }
                        }
                    }

                    db.SubmitChanges();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        private WPost[] GetTimeline(ConnectorDataContext db, User user, List<int> authors, long since, long to)
        {
            List<WPost> result = new List<WPost>();

            try
            {
                if (since == 0 && to != 0)
                {
                    //checked if we have enought older posts
                    DateTime lastPostDate = db.Posts.Where(tp => tp.id == to).Single().createAt;
                    int olderPostCounter = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user) &&
                        p.createAt < lastPostDate).Count();

                    if (olderPostCounter < postLimit)
                    {
                        foreach (int item in authors)
                            DownloadOlderPost(item);
                    }
                }
                else
                {
                    new Thread(delegate()
                        {
                            foreach (int item in authors)
                                DownloadNewerPost(item);
                        }).Start();
                }
            }
            catch (InvalidOperationException)
            {
                return result.ToArray();
            }

            IEnumerable<Post> posts = new List<Post>();

            if (since == 0 && to == 0)
                posts = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user)).OrderByDescending(p => p.createAt).Take(postLimit);
            else if (since != 0)
                posts = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user) && p.createAt > db.Posts.Where(sp => sp.id == since).Single().createAt).OrderByDescending(p => p.createAt).Take(postLimit);
            else
                posts = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user) && p.createAt < db.Posts.Where(tp => tp.id == to).Single().createAt).OrderByDescending(p => p.createAt).Take(postLimit);

            IEnumerable<int> followings = db.StaticFriends.Where(f => f.user == user.id).Select(f => f.friend);

            foreach (Post post in posts)
            {
                result.Add(Converter.PostToWPost(db, user, post));
            }
            return result.ToArray();
        }

        public bool Post(String username, String password, String message)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));
            Contract.Requires(!String.IsNullOrEmpty(message));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",P");

            int service = db.ServiceInstances.Where(si => si.Service.name == "SocialTFS").Single().id;

            long chosenFeature = -1;

            try
            {
                chosenFeature = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.serviceInstance == service && cf.feature == FeaturesType.Post.ToString()).First().id;
            }
            catch (InvalidOperationException)
            {
                try
                {
                    db.Registrations.Where(r => r.user == user.id && r.serviceInstance == service).Single();
                }
                catch
                {
                    Registration registration = new Registration()
                    {
                        User = user,
                        serviceInstance = db.ServiceInstances.Where(si => si.Service.name == "SocialTFS").Single().id,
                        nameOnService = username,
                        idOnService = username
                    };
                    db.Registrations.InsertOnSubmit(registration);
                    db.SubmitChanges();
                }

                ChosenFeature newChoseFeature = new ChosenFeature()
                {
                    Registration = db.Registrations.Where(r => r.user == user.id && r.serviceInstance == service).Single(),
                    feature = FeaturesType.Post.ToString(),
                    lastDownload = new DateTime(1900, 1, 1)
                };

                db.ChosenFeatures.InsertOnSubmit(newChoseFeature);
                db.SubmitChanges();
                chosenFeature = newChoseFeature.id;
            }

            db.Posts.InsertOnSubmit(new Post
            {
                chosenFeature = chosenFeature,
                message = message,
                createAt = DateTime.UtcNow
            });

            db.SubmitChanges();

            return true;

        }

        private User CheckCredentials(ConnectorDataContext db, String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            try
            {
                User user = db.Users.Where(u => u.username == username && u.password == db.Encrypt(password) && u.active).Single();
                return user;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private void DownloadNewerPost(int author)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            List<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(cf =>
                cf.Registration.user == author &&
                cf.feature == FeaturesType.UserTimeline.ToString()).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                ChosenFeature cfTemp = db.ChosenFeatures.Where(cf => cf.id == item.id).Single();
                if (cfTemp.lastDownload >= DateTime.UtcNow - _postSpan)
                    continue;
                else
                    cfTemp.lastDownload = DateTime.UtcNow;

                try { db.SubmitChanges(); }
                catch { continue; }

                long sinceId;
                DateTime sinceDate = new DateTime();
                try
                {
                    Post sincePost = db.Posts.Where(p => p.chosenFeature == cfTemp.id).OrderByDescending(p => p.createAt).First();
                    sinceId = sincePost.idOnService.GetValueOrDefault();
                    sinceDate = sincePost.createAt;
                }
                catch (InvalidOperationException)
                {
                    sinceId = 0;
                }

                IService service = ServiceFactory.getServiceOauth(
                    cfTemp.Registration.ServiceInstance.Service.name,
                    cfTemp.Registration.ServiceInstance.host,
                    cfTemp.Registration.ServiceInstance.consumerKey,
                    cfTemp.Registration.ServiceInstance.consumerSecret,
                    cfTemp.Registration.accessToken,
                    cfTemp.Registration.accessSecret);
                IPost[] timeline = new IPost[0];

                if (service.Name.Equals("Facebook"))
                {
                    timeline = (IPost[])service.Get(FeaturesType.UserTimeline, long.Parse(cfTemp.Registration.idOnService), sinceId, sinceDate);
                }
                else
                {
                    timeline = (IPost[])service.Get(FeaturesType.UserTimeline, int.Parse(cfTemp.Registration.idOnService), sinceId, sinceDate);
                }


                IEnumerable<long?> postInDb = db.Posts.Where(p => p.chosenFeature == item.id).Select(p => p.idOnService);

                if (timeline != null) 
                    foreach (IPost post in timeline)
                    {
                        if (!postInDb.Contains(post.Id))
                            db.Posts.InsertOnSubmit(new Post
                            {
                                chosenFeature = cfTemp.id,
                                idOnService = post.Id,
                                message = post.Text,
                                createAt = post.CreatedAt
                            });
                    }
            }
            try { 
                db.SubmitChanges(); 
            }
            catch(Exception e) { 
                
            }

        }

        private void DownloadOlderPost(int author)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            List<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(cf =>
                cf.Registration.user == author &&
                cf.feature == FeaturesType.UserTimeline.ToString()).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                long maxId;
                DateTime maxDate = new DateTime();
                try
                {
                    Post maxPost = db.Posts.Where(p => p.chosenFeature == item.id).OrderBy(p => p.createAt).First();
                    maxId = maxPost.idOnService.GetValueOrDefault();
                    maxDate = maxPost.createAt;
                }
                catch (InvalidOperationException)
                {
                    maxId = 0;
                }

                IService service = ServiceFactory.getServiceOauth(
                    item.Registration.ServiceInstance.Service.name,
                    item.Registration.ServiceInstance.host,
                    item.Registration.ServiceInstance.consumerKey,
                    item.Registration.ServiceInstance.consumerSecret,
                    item.Registration.accessToken,
                    item.Registration.accessSecret);

                IPost[] timeline = null;
                if (service.Name.Equals("Facebook"))
                {
                    timeline = (IPost[])service.Get(FeaturesType.UserTimelineOlderPosts, long.Parse(item.Registration.idOnService), maxId, maxDate);
                }
                else
                {
                    timeline = (IPost[])service.Get(FeaturesType.UserTimelineOlderPosts, int.Parse(item.Registration.idOnService), maxId, maxDate);
                }
                IEnumerable<long?> postInDb = db.Posts.Where(p => p.chosenFeature == item.id).Select(p => p.idOnService);

                foreach (IPost post in timeline)
                {
                    if (!postInDb.Contains(post.Id))
                        db.Posts.InsertOnSubmit(new Post
                        {
                            chosenFeature = item.id,
                            idOnService = post.Id,
                            message = post.Text,
                            createAt = post.CreatedAt
                        });
                }
            }
            db.SubmitChanges();
        }

        public bool Follow(string username, string password, int followId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                db.StaticFriends.InsertOnSubmit(new StaticFriend()
                {
                    user = user.id,
                    friend = followId
                });

                db.SubmitChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Unfollow(string username, string password, int followId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                StaticFriend friend = db.StaticFriends.Where(f => f.user == user.id && f.friend == followId).Single();

                db.StaticFriends.DeleteOnSubmit(friend);
                db.SubmitChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public WUser[] GetFollowings(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",FG");

            List<WUser> users = new List<WUser>();

            foreach (StaticFriend item in db.StaticFriends.Where(sf => sf.User == user))
            {
                users.Add(Converter.UserToWUser(db, user, item.Friend, false));
            }

            return users.ToArray();
        }

        public WUser[] GetFollowers(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",FR");

            List<WUser> users = new List<WUser>();
            IEnumerable<int> followings = db.StaticFriends.Where(f => f.user == user.id).Select(f => f.friend);

            foreach (StaticFriend item in db.StaticFriends.Where(f => f.Friend == user))
            {
                users.Add(Converter.UserToWUser(db, user, item.User, false));
            }

            return users.ToArray();
        }

        public WUser[] GetSuggestedFriends(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",SF");

            List<WUser> suggestedFriends = new List<WUser>();

            // Provides all suggested user not in Hidden or in StaticFriend, ordered by the sum of scores
            List<User> suggestion =
            (from s in db.Suggestions
             join fs in db.FeatureScores
                 on new { s.ChosenFeature.feature, s.ChosenFeature.serviceInstance }
                 equals new { fs.feature, fs.serviceInstance }
             where s.ChosenFeature.user == user.id &&
                 !db.Hiddens.Where(h => h.user == s.ChosenFeature.user && h.timeline == HiddenType.Suggestions.ToString()).Select(h => h.friend).Contains(s.user) &&
                 !db.StaticFriends.Where(sf => sf.user == s.ChosenFeature.user).Select(sf => sf.friend).Contains(s.user)
             let key = new { s.User }
             group fs by key into friend
             orderby friend.Sum(af => af.score) descending
             select friend.Key.User).ToList();

            foreach (User item in suggestion)
            {
                suggestedFriends.Add(Converter.UserToWUser(db, user, item, false));
            }

            new Thread(thread => UpdateSuggestion(user)).Start();

            return suggestedFriends.ToArray();
        }

        private void UpdateSuggestion(User user)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            Dictionary<int, HashSet<int>> logFriends = new Dictionary<int, HashSet<int>>();
            bool needToLog = false;

            IEnumerable<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(
                cf => (cf.feature.Equals(FeaturesType.Followings.ToString()) ||
                    cf.feature.Equals(FeaturesType.Followers.ToString()) ||
                    cf.feature.Equals(FeaturesType.TFSCollection.ToString()) ||
                    cf.feature.Equals(FeaturesType.TFSTeamProject.ToString())) && cf.user == user.id);

            foreach (ChosenFeature chosenFeature in chosenFeatures)
            {
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                if (temp.lastDownload > DateTime.UtcNow - _suggestionSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try
                {
                    db.SubmitChanges();
                    needToLog = true;
                }
                catch
                {
                    continue;
                }

                IService service = null;

                if (chosenFeature.feature.Equals(FeaturesType.Followings.ToString()) ||
                    chosenFeature.feature.Equals(FeaturesType.Followers.ToString()))
                    service = ServiceFactory.getServiceOauth(
                        chosenFeature.Registration.ServiceInstance.Service.name,
                        chosenFeature.Registration.ServiceInstance.host,
                        chosenFeature.Registration.ServiceInstance.consumerKey,
                        chosenFeature.Registration.ServiceInstance.consumerSecret,
                        chosenFeature.Registration.accessToken,
                        chosenFeature.Registration.accessSecret);
                else if (chosenFeature.feature.Equals(FeaturesType.TFSCollection.ToString()) ||
                    chosenFeature.feature.Equals(FeaturesType.TFSTeamProject.ToString()))
                {
                    if (temp.Registration.ServiceInstance.Service.name.Equals("GitHub"))
                    {
                        service = ServiceFactory.getServiceOauth(temp.Registration.ServiceInstance.Service.name, temp.Registration.ServiceInstance.host, temp.Registration.ServiceInstance.consumerKey, temp.Registration.ServiceInstance.consumerSecret, temp.Registration.accessToken, null);
                    }
                    else
                    {
                        service = ServiceFactory.getService(
                            chosenFeature.Registration.ServiceInstance.Service.name,
                            chosenFeature.Registration.nameOnService,
                            db.EncDecRc4("key", chosenFeature.Registration.accessToken),
                            chosenFeature.Registration.accessSecret,
                            chosenFeature.Registration.ServiceInstance.host);
                    }
                }

                string[] friends = null;

                if (chosenFeature.feature.Equals(FeaturesType.Followings.ToString()))
                    friends = (string[])service.Get(FeaturesType.Followings, null);
                else if (chosenFeature.feature.Equals(FeaturesType.Followers.ToString()))
                    friends = (string[])service.Get(FeaturesType.Followers, null);
                else if (chosenFeature.feature.Equals(FeaturesType.TFSCollection.ToString()))
                    friends = (string[])service.Get(FeaturesType.TFSCollection, null);
                else if (chosenFeature.feature.Equals(FeaturesType.TFSTeamProject.ToString()))
                    friends = (string[])service.Get(FeaturesType.TFSTeamProject, null);

                if (friends != null)
                {
                    //Delete suggestions for this chosen feature in the database
                    db.Suggestions.DeleteAllOnSubmit(db.Suggestions.Where(s => s.chosenFeature == chosenFeature.id));
                    db.SubmitChanges();

                    foreach (string friend in friends)
                    {
                        IEnumerable<User> friendInSocialTfs = db.Registrations.Where(r => r.idOnService == friend &&
                            r.serviceInstance == chosenFeature.serviceInstance).Select(r => r.User);

                        if (friendInSocialTfs.Count() == 1)
                        {
                            User suggestedFriend = friendInSocialTfs.First();

                            if (friend != chosenFeature.Registration.idOnService)
                            {
                                db.Suggestions.InsertOnSubmit(new Suggestion()
                                {
                                    user = suggestedFriend.id,
                                    chosenFeature = chosenFeature.id
                                });

                                if (!logFriends.ContainsKey(suggestedFriend.id))
                                    logFriends[suggestedFriend.id] = new HashSet<int>();
                                logFriends[suggestedFriend.id].Add(temp.Registration.serviceInstance);
                            }
                        }
                    }
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch { }
                }
            }

            if (needToLog)
            {
                ILog log = LogManager.GetLogger("NetworkLogger");
                log.Info(user.id + ",S,[" + GetFriendString(user.id, logFriends) + "]");
            }
        }

        public string[] GetSkills(string username, string password, string ownerName)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new string[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",SK");

            User owner;

            try
            {
                owner = db.Users.Where(u => u.username == ownerName).Single();
            }
            catch (Exception)
            {
                return new string[0];
            }

            DownloadSkills(owner, db);

            //get the names of the skills from the database
            IEnumerable<string> skills = db.Skills.Where(s => s.ChosenFeature.user == owner.id).Select(s => s.skill);

            return skills.Distinct().ToArray<string>();
        }

        private void DownloadSkills(User currentUser, ConnectorDataContext db)
        {
            List<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(cf => cf.user == currentUser.id &&
                cf.feature == FeaturesType.Skills.ToString() &&
                cf.lastDownload < DateTime.UtcNow - _skillSpan).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                //delete the user's skills in the database
                db.Skills.DeleteAllOnSubmit(item.Skills);

                db.SubmitChanges();

                Registration registration = item.Registration;
                IService service = ServiceFactory.getServiceOauth(
                    registration.ServiceInstance.Service.name,
                    registration.ServiceInstance.host,
                    registration.ServiceInstance.consumerKey,
                    registration.ServiceInstance.consumerSecret,
                    registration.accessToken,
                    registration.accessSecret);

                string[] userSkills = (string[])service.Get(FeaturesType.Skills, null);

                //insert skills in the database
                foreach (string userSkill in userSkills)
                {
                    db.Skills.InsertOnSubmit(new Skill()
                    {
                        chosenFeature = item.id,
                        skill = userSkill
                    });
                }

                item.lastDownload = DateTime.UtcNow;

                db.SubmitChanges();
            }
        }

        public bool UpdateChosenFeatures(string username, string password, int serviceInstanceId, string[] chosenFeatures)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            bool suggestion = false, dynamic = false, interactive = false;

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            //remove the old chosen features
            IEnumerable<ChosenFeature> chosenFeaturesToDelete = db.ChosenFeatures.Where(c => c.user == user.id
                && !chosenFeatures.Contains(c.feature) && c.serviceInstance == serviceInstanceId);
            db.ChosenFeatures.DeleteAllOnSubmit(chosenFeaturesToDelete);
            db.SubmitChanges();

            //add the new chosen features
            foreach (string chosenFeature in chosenFeatures)
            {
                if (!db.ChosenFeatures.Where(c => c.user == user.id && c.feature == chosenFeature && c.serviceInstance == serviceInstanceId).Any())
                {
                    if (chosenFeature == FeaturesType.Followers.ToString()
                        || chosenFeature == FeaturesType.Followings.ToString()
                        || chosenFeature == FeaturesType.TFSTeamProject.ToString()
                        || chosenFeature == FeaturesType.TFSTeamProject.ToString())
                        suggestion = true;
                    else if (chosenFeature == FeaturesType.IterationNetwork.ToString())
                        dynamic = true;
                    else if (chosenFeature == FeaturesType.InteractiveNetwork.ToString())
                        interactive = true;
                    db.ChosenFeatures.InsertOnSubmit(new ChosenFeature()
                    {
                        user = user.id,
                        serviceInstance = serviceInstanceId,
                        feature = chosenFeature,
                        lastDownload = new DateTime(1900, 1, 1)
                    });
                }
            }
            db.SubmitChanges();

            if (suggestion)
                new Thread(thread => UpdateSuggestion(user)).Start();

            if (dynamic)
                new Thread(thread => UpdateDynamicFriend(user)).Start();

            if (interactive)
                new Thread(thread => UpdateInteractiveFriend(user)).Start();

            return true;
        }

        public WFeature[] GetChosenFeatures(string username, string password, int serviceInstanceId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WFeature[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",CF");

            List<WFeature> result = new List<WFeature>();

            foreach (FeaturesType item in ServiceFactory.getService(
                db.ServiceInstances.Where(si => si.id == serviceInstanceId).Single().Service.name).GetPublicFeatures())
            {
                WFeature feature = new WFeature()
                {
                    Name = item.ToString(),
                    Description = FeaturesManager.GetFeatureDescription(item),
                    IsChosen = db.ChosenFeatures.Where(cf => cf.serviceInstance == serviceInstanceId &&
                        cf.user == user.id).Select(cf => cf.feature).Contains(item.ToString())
                };
                result.Add(feature);
            }

            return result.ToArray();
        }

        public WUser[] GetHiddenUsers(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",HU");

            List<WUser> result = new List<WUser>();

            foreach (User item in db.Hiddens.Where(h => h.user == user.id).Select(h => h.Friend).Distinct())
            {
                result.Add(Converter.UserToWUser(db, user, item, false));
            }

            return result.ToArray();
        }

        public WHidden GetUserHideSettings(string username, string password, int userId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return null;

            WHidden result = new WHidden();

            foreach (Hidden item in db.Hiddens.Where(h => h.user == user.id && h.friend == userId))
                if (item.timeline == HiddenType.Suggestions.ToString())
                    result.Suggestions = true;
                else if (item.timeline == HiddenType.Dynamic.ToString())
                    result.Dynamic = true;
                else if (item.timeline == HiddenType.Interactive.ToString())
                    result.Interactive = true;

            return result;
        }

        public bool UpdateHiddenUser(string username, string password, int userId, bool suggestions, bool dynamic, bool interactive)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                db.Hiddens.DeleteAllOnSubmit(db.Hiddens.Where(h => h.user == user.id && h.friend == userId));
                db.SubmitChanges();

                if (suggestions)
                    db.Hiddens.InsertOnSubmit(new Hidden()
                    {
                        user = user.id,
                        friend = userId,
                        timeline = HiddenType.Suggestions.ToString()
                    });

                if (dynamic)
                    db.Hiddens.InsertOnSubmit(new Hidden()
                    {
                        user = user.id,
                        friend = userId,
                        timeline = HiddenType.Dynamic.ToString()
                    });

                if (interactive)
                    db.Hiddens.InsertOnSubmit(new Hidden()
                    {
                        user = user.id,
                        friend = userId,
                        timeline = HiddenType.Interactive.ToString()
                    });

                db.SubmitChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Uri[] GetAvailableAvatars(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new Uri[0];

            ILog log = LogManager.GetLogger("PanelLogger");
            log.Info(user.id + ",AA");

            List<Uri> avatars = new List<Uri>();
            foreach (ChosenFeature chosenFeature in db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.Avatar.ToString()))
            {
                Registration registration = chosenFeature.Registration;
                IService service = ServiceFactory.getServiceOauth(
                    registration.ServiceInstance.Service.name,
                    registration.ServiceInstance.host,
                    registration.ServiceInstance.consumerKey,
                    registration.ServiceInstance.consumerSecret,
                    registration.accessToken,
                    registration.accessSecret);
                Uri avatar = null;
                try { avatar = (Uri)service.Get(FeaturesType.Avatar); }
                catch (Exception) { }

                if (avatar != null)
                {
                    avatars.Add(avatar);
                }
            }

            return avatars.ToArray();
        }

        public bool SaveAvatar(string username, string password, Uri avatar)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            ConnectorDataContext db = new ConnectorDataContext();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            user.avatar = avatar.AbsoluteUri;
            db.SubmitChanges();

            return true;
        }
    }
}
