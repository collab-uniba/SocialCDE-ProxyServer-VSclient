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
using System.Diagnostics;


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
                Stopwatch w = Stopwatch.StartNew();
                bool feat = db.Features.Contains(new Feature() { name = featureType.ToString() });
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", feature's name: " + featureType.ToString() + ", check if the feature is available when the application is started");
                if (!feat)
                {
                    Stopwatch w1 = Stopwatch.StartNew();
                    db.Features.InsertOnSubmit(new Feature()
                    {
                        name = featureType.ToString(),
                        description = FeaturesManager.GetFeatureDescription(featureType),
                        @public = FeaturesManager.IsPublicFeature(featureType)
                    });
                    w1.Stop();
                    ILog log1 = LogManager.GetLogger("QueryLogger");
                    log1.Info(" Elapsed time: " + w1.Elapsed + ", feature's name: " + featureType.ToString() + ", description: " + FeaturesManager.GetFeatureDescription(featureType) + ", public: " + FeaturesManager.IsPublicFeature(featureType) + ", insert a feature in a pending state");
                }
            }
            Stopwatch w2 = Stopwatch.StartNew();
            db.SubmitChanges();
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", insert the feature");
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
                Stopwatch w = Stopwatch.StartNew();
                User user = db.Users.Where(u => u.username == username && u.active).Single();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", username: " + username + ", select an username to check if it is already used");
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
                Stopwatch w = Stopwatch.StartNew();
                user = db.Users.Where(u => u.email == email).Single();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user email: " + email + ", select the user to subscribe him");
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

            Stopwatch w1 = Stopwatch.StartNew();
            int sInstance = db.ServiceInstances.Where(si => si.Service.name == "SocialTFS").Single().id;
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", select the service instance with name 'SocialTFS'");

            Registration registration = new Registration()
            {
                User = user,
                serviceInstance = sInstance,
                nameOnService = username,
                idOnService = username
            };
            Stopwatch w2 = Stopwatch.StartNew();
            db.Registrations.InsertOnSubmit(registration);
            db.SubmitChanges();
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", service instance's id: " + sInstance + ", name and id on service: " + username + ", insert a new registration");

            Stopwatch w3 = Stopwatch.StartNew();
            db.ChosenFeatures.InsertOnSubmit(new ChosenFeature()
            {
                Registration = registration,
                feature = FeaturesType.Post.ToString(),
                lastDownload = new DateTime(1900, 1, 1)
            });
            db.SubmitChanges();
            w3.Stop();
            ILog log3 = LogManager.GetLogger("QueryLogger");
            log3.Info(" Elapsed time: " + w3.Elapsed + ", feature: " + FeaturesType.Post.ToString() + ", last download: " + new DateTime(1900, 1, 1) + ", insert a new Chosen feature");

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

            Stopwatch w = Stopwatch.StartNew();
            user.password = db.Encrypt(newPassword);
            db.SubmitChanges();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", change password");
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

            Stopwatch w1 = Stopwatch.StartNew();
            List<ServiceInstance> sInstance = db.ServiceInstances.Where(si => si.Service.name != "SocialTFS").ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", select all service instances with the service different from 'SocialTFS'");

            foreach (ServiceInstance item in sInstance)
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
                Stopwatch w1 = Stopwatch.StartNew();
                colleague = db.Users.Where(u => u.id == colleagueId).Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", collegue id: " + colleagueId + ", select user profile");
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

            Stopwatch w = Stopwatch.StartNew();
            ServiceInstance si = db.ServiceInstances.Where(s => s.id == service).Single();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", service instance: " + service + ", select service instance for OAuth 1 authentication procedure");

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

            Stopwatch w = Stopwatch.StartNew();
            ServiceInstance si = db.ServiceInstances.Where(s => s.id == service).Single();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", service id: " + service + ", select service id from serviceinstance");
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

            Stopwatch w = Stopwatch.StartNew();
            ServiceInstance serviceInstance = db.ServiceInstances.Where(s => s.id == service).Single();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", service id: " + service + ", record a service without OAuth authentication procedure");

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
                reg.nameOnService = iUser.UserName != null ? iUser.UserName : iUser.Id;
                reg.idOnService = iUser.Id.ToString();
                reg.accessToken = accessToken;
                reg.accessSecret = accessSecret;


                Stopwatch w = Stopwatch.StartNew();
                db.Registrations.InsertOnSubmit(reg);
                db.SubmitChanges();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", id on service: " + iUser.Id.ToString() + ", access token: " + accessToken + ", access secret: " + accessSecret + ", register user on a service");
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
                Stopwatch w = Stopwatch.StartNew();
                db.Registrations.DeleteAllOnSubmit(db.Registrations.Where(r => r.user == user.id && r.serviceInstance == service));
                db.SubmitChanges();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", service id: " + service + ", unsubscribe service");
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

            Stopwatch w1 = Stopwatch.StartNew();
            List<int> authors = db.StaticFriends.Where(f => f.user == user.id).Select(f => f.friend).ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", select all friends of an author(home timeline)");
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
            Stopwatch w1 = Stopwatch.StartNew();
            List<int> authors = new List<int> { db.Users.Where(u => u.username == ownerName).Single().id };
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", username: " + ownerName + ", select all users' ids to get their posts");

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

            Stopwatch w1 = Stopwatch.StartNew();
            List<int> hiddenAuthors = db.Hiddens.Where(h => h.user == user.id && h.timeline == HiddenType.Dynamic.ToString()).Select(h => h.friend).ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", timeline: " + HiddenType.Dynamic.ToString() + ", select all dynamic friends hidden by an user in the iteration timeline");
            Stopwatch w2 = Stopwatch.StartNew();
            List<int> authors = db.DynamicFriends.Where(f => f.ChosenFeature.user == user.id && !hiddenAuthors.Contains(f.user)).Select(f => f.user).ToList();
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", user id: " + user.id + ", select all users whose posts can appear in the iteration timeline of an user");
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

            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> cFeature = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.IterationNetwork.ToString()).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", feature's name: " + FeaturesType.IterationNetwork.ToString() + ", select all chosen feature of an author and his feature 'Iteration Network'");

            foreach (ChosenFeature chosenFeature in cFeature)
            {
                Stopwatch w1 = Stopwatch.StartNew();
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", chosen feature's id: " + chosenFeature.id + ", select a chosen feature to update");
                if (temp.lastDownload > DateTime.UtcNow - _dynamicSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try
                {
                    Stopwatch w3 = Stopwatch.StartNew();
                    db.SubmitChanges();
                    w3.Stop();
                    ILog log3 = LogManager.GetLogger("QueryLogger");
                    log3.Info(" Elapsed time: " + w3.Elapsed + ", update the chosen feature according to the date(dynamic friend)");
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
                Stopwatch w4 = Stopwatch.StartNew();
                db.DynamicFriends.DeleteAllOnSubmit(db.DynamicFriends.Where(df => df.chosenFeature == temp.id));
                db.SubmitChanges();
                w4.Stop();
                ILog log4 = LogManager.GetLogger("QueryLogger");
                log4.Info(" Elapsed time: " + w4.Elapsed + ", chosen feature's id: " + temp.id + ", delete old friendship for the current chosen feature");

                foreach (String dynamicFriend in dynamicFriends)
                {
                    Stopwatch w5 = Stopwatch.StartNew();
                    IEnumerable<int> friendsInDb = db.Registrations.Where(r => r.nameOnService == dynamicFriend && r.serviceInstance == temp.serviceInstance).Select(r => r.user);
                    w5.Stop();
                    ILog log5 = LogManager.GetLogger("QueryLogger");
                    log5.Info(" Elapsed time: " + w5.Elapsed + ", : dynamic friend" + dynamicFriend + ", service instance: " + temp.serviceInstance + ", select user to add as dynamic friend");
                    foreach (int friendInDb in friendsInDb)
                    {
                        Stopwatch w6 = Stopwatch.StartNew();
                        db.DynamicFriends.InsertOnSubmit(new DynamicFriend()
                        {
                            chosenFeature = temp.id,
                            user = friendInDb
                        });
                        w6.Stop();
                        ILog log6 = LogManager.GetLogger("QueryLogger");
                        log6.Info(" Elapsed time: " + w6.Elapsed + ", chosen feature's id: " + temp.id + ", friend id: " + friendInDb + ", insert a new dynamic friend in a pending state");

                        if (!logFriends.ContainsKey(friendInDb))
                            logFriends[friendInDb] = new HashSet<int>();
                        logFriends[friendInDb].Add(temp.Registration.serviceInstance);
                    }
                }
                try
                {
                    Stopwatch w7 = Stopwatch.StartNew();
                    db.SubmitChanges();
                    w7.Stop();
                    ILog log7 = LogManager.GetLogger("QueryLogger");
                    log7.Info(" Elapsed time: " + w7.Elapsed + ", insert a dynamic friend");
                }
                catch { }
            }

            if (needToLog)
            {
                ILog log8 = LogManager.GetLogger("NetworkLogger");
                log8.Info(user.id + ",I,[" + GetFriendString(user.id, logFriends) + "]");
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
            }
            else if (collectionUri.Contains("git://"))
            { 
                //in this case the interactive object is an github issue
                GetUsersIssuesInvolved(user, collectionUri, interactiveObject);
            }

            Stopwatch w1 = Stopwatch.StartNew();
            List<int> hiddenAuthors = db.Hiddens.Where(h => h.user == user.id && h.timeline == HiddenType.Interactive.ToString()).Select(h => h.friend).ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", timeline: " + HiddenType.Interactive.ToString() + ", select all friends hidden by an user in the interactive timeline");
            Stopwatch w2 = Stopwatch.StartNew();
            List<int> authors = db.InteractiveFriends.Where(f => /*f.ChosenFeature.user == user.id && */f.collection == collectionUri && f.interactiveObject.EndsWith(interactiveObject) && f.objectType == objectType && !hiddenAuthors.Contains(f.user)).Select(f => f.user).ToList();
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", collection of projects: " + collectionUri + ", interactive object: " + interactiveObject + ", object type: " + objectType + ", select all users whose posts can appear in the interactive timeline of an user");
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
            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> cFeatures = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.InteractiveNetwork.ToString()).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", feature: " + FeaturesType.InteractiveNetwork.ToString() + ", select all chosen features of an user with feature 'InteractiveNetwork'");
            foreach (ChosenFeature chosenFeature in cFeatures)
            {
                Stopwatch w1 = Stopwatch.StartNew();
                temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", chosen feature's id: " + chosenFeature.id + ", select a chosen feature to get users' issues involved");
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

                Stopwatch w2 = Stopwatch.StartNew();
                db.InteractiveFriends.DeleteAllOnSubmit(db.InteractiveFriends.Where(intFr => intFr.chosenFeature == temp.id && intFr.objectType == "WorkItem"));
                db.SubmitChanges();
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", chosen feature's id: " + temp.id + ", delete all interactive friends according to the feature and objecttype 'WorkItem'");
                Stopwatch w3 = Stopwatch.StartNew();
                IEnumerable<int> friendsInDb = db.Registrations.Where(r => workitem.InvolvedUsers.Contains(r.nameOnService) || workitem.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.user);
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", select all users that are working on the same workitem(GetUsersIssuesInvolved)");
                foreach (int friendInDb in friendsInDb)
                {
                    Stopwatch w4 = Stopwatch.StartNew();
                    db.InteractiveFriends.InsertOnSubmit(new InteractiveFriend()
                    {
                        user = friendInDb,
                        chosenFeature = temp.id,
                        collection = collectionUri,
                        interactiveObject = workitem.Name,
                        objectType = "WorkItem"
                    });
                    w4.Stop();
                    ILog log4 = LogManager.GetLogger("QueryLogger");
                    log4.Info(" Elapsed time: " + w4.Elapsed + ", user id: " + friendInDb + ", chosen feature: " + temp.id + ", collection uri: " + collectionUri + ", interactive object: " + workitem.Name + ", insert an interactive friend which is working on a workitem in a pending state");
                }

                Stopwatch w8 = Stopwatch.StartNew();
                db.SubmitChanges();
                w8.Stop();
                ILog log8 = LogManager.GetLogger("QueryLogger");
                log8.Info(" Elapsed time: " + w8.Elapsed + ", insert the interactive friend");
            }

        }

        private string FindGithubRepository(User user, string interactiveObject)
        {
            ConnectorDataContext db = new ConnectorDataContext();
            Boolean flag = false;
            IService service = null;

            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> cFeature = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.InteractiveNetwork.ToString()).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", feature's name: " + FeaturesType.InteractiveNetwork.ToString() + ", select all chosen features of an user and his feature 'interactive network'");

            foreach (ChosenFeature chosenFeature in cFeature)
            {
                Stopwatch w1 = Stopwatch.StartNew();
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", chosen feature's id: " + chosenFeature.id + ", select chosen feature's id");

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

            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> cFeature = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.InteractiveNetwork.ToString()).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", feature's name: " + FeaturesType.InteractiveNetwork.ToString() + ", select all chosen features of an author and his feature 'interactive network'");

            foreach (ChosenFeature chosenFeature in cFeature)
            {
                Stopwatch w2 = Stopwatch.StartNew();
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", chosen feature's id: " + chosenFeature.id + ", select a chosen feature");
                if (temp.lastDownload > DateTime.UtcNow - _interactiveSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try
                {
                    Stopwatch w1 = Stopwatch.StartNew();
                    db.SubmitChanges();
                    w1.Stop();
                    ILog log1 = LogManager.GetLogger("QueryLogger");
                    log1.Info(" Elapsed time: " + w1.Elapsed + ", update the chosen feature according to the date(interactive friend)");
                }
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
                    Stopwatch w3 = Stopwatch.StartNew();
                    db.InteractiveFriends.DeleteAllOnSubmit(db.InteractiveFriends.Where(intFr => intFr.chosenFeature == temp.id));
                    db.SubmitChanges();
                    w3.Stop();
                    ILog log3 = LogManager.GetLogger("QueryLogger");
                    log3.Info(" Elapsed time: " + w3.Elapsed + ", chosen feature's id: " + temp.id + ", remove all old interactive friends according to the chosen feature");

                    foreach (SCollection collection in collections)
                    {
                        foreach (SWorkItem workitem in collection.WorkItems)
                        {
                            Stopwatch w4 = Stopwatch.StartNew();
                            IEnumerable<int> friendsInDb = db.Registrations.Where(r => workitem.InvolvedUsers.Contains(r.nameOnService) || workitem.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.user);
                            w4.Stop();
                            ILog log4 = LogManager.GetLogger("QueryLogger");
                            log4.Info(" Elapsed time: " + w4.Elapsed + ", select all users that are working on the same workitem");
                            foreach (int friendInDb in friendsInDb)
                            {
                                Stopwatch w5 = Stopwatch.StartNew();
                                db.InteractiveFriends.InsertOnSubmit(new InteractiveFriend()
                                {
                                    user = friendInDb,
                                    chosenFeature = temp.id,
                                    collection = collection.Uri,
                                    interactiveObject = workitem.Name,
                                    objectType = "WorkItem"
                                });
                                w5.Stop();
                                ILog log5 = LogManager.GetLogger("QueryLogger");
                                log5.Info(" Elapsed time: " + w5.Elapsed + ", user id: " + friendInDb + ", chosen feature: " + temp.id + ", collection uri: " + collection.Uri + ", interactive object: " + workitem.Name + ", insert an interactive friend which is working on a workitem in a pending state");
                            }
                        }
                        foreach (SFile file in collection.Files)
                        {
                            Stopwatch w6 = Stopwatch.StartNew();
                            IEnumerable<int> friendsInDb = db.Registrations.Where(r => file.InvolvedUsers.Contains(r.nameOnService) || file.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.user);
                            w6.Stop();
                            ILog log6 = LogManager.GetLogger("QueryLogger");
                            log6.Info(" Elapsed time: " + w6.Elapsed + ", select all users that are working on the same file");
                            foreach (int friendInDb in friendsInDb)
                            {
                                Stopwatch w7 = Stopwatch.StartNew();
                                db.InteractiveFriends.InsertOnSubmit(new InteractiveFriend()
                                {
                                    user = friendInDb,
                                    chosenFeature = temp.id,
                                    collection = collection.Uri,
                                    interactiveObject = file.Name,
                                    objectType = "File"
                                });
                                w7.Stop();
                                ILog log7 = LogManager.GetLogger("QueryLogger");
                                log7.Info(" Elapsed time: " + w7.Elapsed + ", user id: " + friendInDb + ", chosen feature: " + temp.id + ", collection uri: " + collection.Uri + ", interactive object: " + file.Name + ", insert an interactive friend which is working on a file in a pending state");
                            }
                        }
                    }

                    Stopwatch w8 = Stopwatch.StartNew();
                    db.SubmitChanges();
                    w8.Stop();
                    ILog log8 = LogManager.GetLogger("QueryLogger");
                    log8.Info(" Elapsed time: " + w8.Elapsed + ", insert the interactive friend");
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
                    Stopwatch w = Stopwatch.StartNew();
                    DateTime lastPostDate = db.Posts.Where(tp => tp.id == to).Single().createAt;
                    w.Stop();
                    ILog log = LogManager.GetLogger("QueryLogger");
                    log.Info(" Elapsed time: " + w.Elapsed + ", post id: " + to + ", select last post");
                    Stopwatch w1 = Stopwatch.StartNew();
                    int olderPostCounter = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user) &&
                        p.createAt < lastPostDate).Count();
                    w1.Stop();
                    ILog log1 = LogManager.GetLogger("QueryLogger");
                    log1.Info(" Elapsed time: " + w1.Elapsed + ", number of posts before a certain post written by an user using a certain service");

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
            {
                Stopwatch w2 = Stopwatch.StartNew();
                posts = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user)).OrderByDescending(p => p.createAt).Take(postLimit);
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", select top posts of an user");
            }
            else if (since != 0)
            {
                Stopwatch w3 = Stopwatch.StartNew();
                posts = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user) && p.createAt > db.Posts.Where(sp => sp.id == since).Single().createAt).OrderByDescending(p => p.createAt).Take(postLimit);
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", post id: " + since + ", select top posts of an user chronologically written after a certain post");
            }
            else
            {
                Stopwatch w4 = Stopwatch.StartNew();
                posts = db.Posts.Where(p => authors.Contains(p.ChosenFeature.Registration.user) && p.createAt < db.Posts.Where(tp => tp.id == to).Single().createAt).OrderByDescending(p => p.createAt).Take(postLimit);
                w4.Stop();
                ILog log4 = LogManager.GetLogger("QueryLogger");
                log4.Info(" Elapsed time: " + w4.Elapsed + ", post id: " + to + ", select top posts of an user chronologically written before a certain post");
            }
            Stopwatch w5 = Stopwatch.StartNew();
            IEnumerable<int> followings = db.StaticFriends.Where(f => f.user == user.id).Select(f => f.friend);
            w5.Stop();
            ILog log5 = LogManager.GetLogger("QueryLogger");
            log5.Info(" Elapsed time: " + w5.Elapsed + ", user id: " + user.id + ", select all static friends that follow that user"); ;

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

            Stopwatch w1 = Stopwatch.StartNew();
            int service = db.ServiceInstances.Where(si => si.Service.name == "SocialTFS").Single().id;
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", select service instance's id of the service 'SocialTFS'");

            long chosenFeature = -1;

            try
            {
                Stopwatch w2 = Stopwatch.StartNew();
                chosenFeature = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.serviceInstance == service && cf.feature == FeaturesType.Post.ToString()).First().id;
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", user id: " + user.id + ", service instance: " + service + ", feature's name: " + FeaturesType.Post.ToString() + ", select chosen feature's id");
            }
            catch (InvalidOperationException)
            {
                try
                {
                    Stopwatch w3 = Stopwatch.StartNew();
                    db.Registrations.Where(r => r.user == user.id && r.serviceInstance == service).Single();
                    w3.Stop();
                    ILog log3 = LogManager.GetLogger("QueryLogger");
                    log3.Info(" Elapsed time: " + w3.Elapsed + ", user id: " + user.id + ", service instance: " + service + ", select registration of a service");
                }
                catch
                {
                    Registration registration = new Registration()
                    {
                        User = user,
                        serviceInstance = db.ServiceInstances.Where(si => si.Service.name == "SocialTFS").Single().id,  //considerata poco sopra per il log
                        nameOnService = username,
                        idOnService = username
                    };
                    Stopwatch w4 = Stopwatch.StartNew();
                    db.Registrations.InsertOnSubmit(registration);
                    db.SubmitChanges();
                    w4.Stop();
                    ILog log4 = LogManager.GetLogger("QueryLogger");
                    log4.Info(" Elapsed time: " + w4.Elapsed + ", insert a registration");
                }

                ChosenFeature newChoseFeature = new ChosenFeature()
                {
                    Registration = db.Registrations.Where(r => r.user == user.id && r.serviceInstance == service).Single(), //considerata poco sopra per il log
                    feature = FeaturesType.Post.ToString(),
                    lastDownload = new DateTime(1900, 1, 1)
                };
                Stopwatch w5 = Stopwatch.StartNew();
                db.ChosenFeatures.InsertOnSubmit(newChoseFeature);
                db.SubmitChanges();
                w5.Stop();
                ILog log5 = LogManager.GetLogger("QueryLogger");
                log5.Info(" Elapsed time: " + w5.Elapsed + ", feature's name: " + FeaturesType.Post.ToString() + ", last download: " + new DateTime(1900, 1, 1) + ", insert a new chosen feature");
                chosenFeature = newChoseFeature.id;
            }

            Stopwatch w6 = Stopwatch.StartNew();
            db.Posts.InsertOnSubmit(new Post
            {
                chosenFeature = chosenFeature,
                message = message,
                createAt = DateTime.UtcNow
            });

            db.SubmitChanges();
            w6.Stop();
            ILog log6 = LogManager.GetLogger("QueryLogger");
            log6.Info(" Elapsed time: " + w6.Elapsed + ", message: " + message + ", date time: " + DateTime.UtcNow + ", insert the post");

            return true;

        }

        private User CheckCredentials(ConnectorDataContext db, String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            try
            {
                Stopwatch w = Stopwatch.StartNew();
                User user = db.Users.Where(u => u.username == username && u.password == db.Encrypt(password) && u.active).Single();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", username: " + username + ", password: ****** , check credentials");
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

            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(cf =>
                cf.Registration.user == author &&
                cf.feature == FeaturesType.UserTimeline.ToString()).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + author + ", feature's name: " + FeaturesType.UserTimeline.ToString() + ", select all chosen features of an author and his feature 'user timeline'(new post)");

            foreach (ChosenFeature item in chosenFeatures)
            {
                Stopwatch w1 = Stopwatch.StartNew();
                ChosenFeature cfTemp = db.ChosenFeatures.Where(cf => cf.id == item.id).Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", chosen feature's id: " + item.id + ", select a chosen feature");
                if (cfTemp.lastDownload >= DateTime.UtcNow - _postSpan)
                    continue;
                else
                    cfTemp.lastDownload = DateTime.UtcNow;

                try
                {
                    Stopwatch w6 = Stopwatch.StartNew();
                    db.SubmitChanges();
                    w6.Stop();
                    ILog log6 = LogManager.GetLogger("QueryLogger");
                    log6.Info(" Elapsed time: " + w6.Elapsed + ", update the chosen feature according to the date(download newer post)");
                }
                catch { continue; }

                long sinceId;
                DateTime sinceDate = new DateTime();
                try
                {
                    Stopwatch w2 = Stopwatch.StartNew();
                    Post sincePost = db.Posts.Where(p => p.chosenFeature == cfTemp.id).OrderByDescending(p => p.createAt).First();
                    w2.Stop();
                    ILog log2 = LogManager.GetLogger("QueryLogger");
                    log2.Info(" Elapsed time: " + w2.Elapsed + ", chosen feature's id: " + cfTemp.id + ", select the most recent post");
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


                Stopwatch w3 = Stopwatch.StartNew();
                IEnumerable<long?> postInDb = db.Posts.Where(p => p.chosenFeature == item.id).Select(p => p.idOnService);
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", chosen feature's id: " + item.id + ", select id on service of the newer post");

                if (timeline != null)
                    foreach (IPost post in timeline)
                    {
                        if (!postInDb.Contains(post.Id))
                        {
                            Stopwatch w4 = Stopwatch.StartNew();
                            db.Posts.InsertOnSubmit(new Post
                            {
                                chosenFeature = cfTemp.id,
                                idOnService = post.Id,
                                message = post.Text,
                                createAt = post.CreatedAt
                            });
                            w4.Stop();
                            ILog log4 = LogManager.GetLogger("QueryLogger");
                            log4.Info(" Elapsed time: " + w4.Elapsed + ", chosen feature's id: " + cfTemp.id + ", id on service: " + post.Id + ", message: " + post.Text + ", date time of creation: " + post.CreatedAt + ", preparing to insert the newer post");
                        }
                    }
            }
            try
            {
                Stopwatch w5 = Stopwatch.StartNew();
                db.SubmitChanges();
                w5.Stop();
                ILog log5 = LogManager.GetLogger("QueryLogger");
                log5.Info(" Elapsed time: " + w5.Elapsed + ", actually inserting the newer post");
            }
            catch (Exception e)
            {

            }

        }

        private void DownloadOlderPost(int author)
        {
            ConnectorDataContext db = new ConnectorDataContext();

            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(cf =>
                cf.Registration.user == author &&
                cf.feature == FeaturesType.UserTimeline.ToString()).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + author + ", feature's name: " + FeaturesType.UserTimeline.ToString() + ", select all chosen features of an author and his feature 'user timeline'(old post)");

            foreach (ChosenFeature item in chosenFeatures)
            {
                long maxId;
                DateTime maxDate = new DateTime();
                try
                {
                    Stopwatch w1 = Stopwatch.StartNew();
                    Post maxPost = db.Posts.Where(p => p.chosenFeature == item.id).OrderBy(p => p.createAt).First();
                    w1.Stop();
                    ILog log1 = LogManager.GetLogger("QueryLogger");
                    log1.Info(" Elapsed time: " + w1.Elapsed + ", chosen feature's id: " + item.id + ", select the oldest post");
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
                Stopwatch w2 = Stopwatch.StartNew();
                IEnumerable<long?> postInDb = db.Posts.Where(p => p.chosenFeature == item.id).Select(p => p.idOnService);
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", chosen feature's id: " + item.id + ", select id on service of the oldest post");

                foreach (IPost post in timeline)
                {
                    if (!postInDb.Contains(post.Id))
                    {
                        Stopwatch w3 = Stopwatch.StartNew();
                        db.Posts.InsertOnSubmit(new Post
                        {
                            chosenFeature = item.id,
                            idOnService = post.Id,
                            message = post.Text,
                            createAt = post.CreatedAt
                        });
                        w3.Stop();
                        ILog log3 = LogManager.GetLogger("QueryLogger");
                        log3.Info(" Elapsed time: " + w3.Elapsed + ", chosen feature's id: " + item.id + ", id on service: " + post.Id + ", message: " + post.Text + ", date time of creation: " + post.CreatedAt + ", preparing to insert the oldest post");
                    }
                }
            }
            Stopwatch w4 = Stopwatch.StartNew();
            db.SubmitChanges();
            w4.Stop();
            ILog log4 = LogManager.GetLogger("QueryLogger");
            log4.Info(" Elapsed time: " + w4.Elapsed + ", actually inserting the oldest post");
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
                Stopwatch w = Stopwatch.StartNew();
                db.StaticFriends.InsertOnSubmit(new StaticFriend()
                {
                    user = user.id,
                    friend = followId
                });
                db.SubmitChanges();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", static friend's id: " + followId + ", insert an user as static friend");

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
                Stopwatch w = Stopwatch.StartNew();
                StaticFriend friend = db.StaticFriends.Where(f => f.user == user.id && f.friend == followId).Single();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", friend's id: " + followId + ", select a static friend of a user");

                Stopwatch w1 = Stopwatch.StartNew();
                db.StaticFriends.DeleteOnSubmit(friend);
                db.SubmitChanges();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", unfollow an user");

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

            Stopwatch w1 = Stopwatch.StartNew();
            List<StaticFriend> sFriends = db.StaticFriends.Where(sf => sf.User == user).ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", select all users followed by an user");

            foreach (StaticFriend item in sFriends)
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
            Stopwatch w1 = Stopwatch.StartNew();
            IEnumerable<int> followings = db.StaticFriends.Where(f => f.user == user.id).Select(f => f.friend);
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", select all static friends' ids of an user");

            Stopwatch w2 = Stopwatch.StartNew();
            List<StaticFriend> sFriends = db.StaticFriends.Where(f => f.Friend == user).ToList();
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", select all users that follow an user");

            foreach (StaticFriend item in sFriends)
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
            Stopwatch w1 = Stopwatch.StartNew();
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
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", select all suggested user not in Hidden or in StaticFriend ordered by the sum of scores");

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

            Stopwatch w = Stopwatch.StartNew();
            IEnumerable<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(
                cf => (cf.feature.Equals(FeaturesType.Followings.ToString()) ||
                    cf.feature.Equals(FeaturesType.Followers.ToString()) ||
                    cf.feature.Equals(FeaturesType.TFSCollection.ToString()) ||
                    cf.feature.Equals(FeaturesType.TFSTeamProject.ToString())) && cf.user == user.id);
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", select all chosen features of an author and his feature 'followings' or 'followers' or 'TFSCollection' or 'TFSTeamProject'");

            foreach (ChosenFeature chosenFeature in chosenFeatures)
            {
                Stopwatch w2 = Stopwatch.StartNew();
                ChosenFeature temp = db.ChosenFeatures.Where(cf => cf.id == chosenFeature.id).Single();
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", chosen feature's id: " + chosenFeature.id + ", select a chosen feature");
                if (temp.lastDownload > DateTime.UtcNow - _suggestionSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try
                {
                    Stopwatch w1 = Stopwatch.StartNew();
                    db.SubmitChanges();
                    w1.Stop();
                    ILog log1 = LogManager.GetLogger("QueryLogger");
                    log1.Info(" Elapsed time: " + w1.Elapsed + ", update the chosen feature according to the date(suggestion)");
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
                    Stopwatch w3 = Stopwatch.StartNew();
                    db.Suggestions.DeleteAllOnSubmit(db.Suggestions.Where(s => s.chosenFeature == chosenFeature.id));
                    db.SubmitChanges();
                    w3.Stop();
                    ILog log3 = LogManager.GetLogger("QueryLogger");
                    log3.Info(" Elapsed time: " + w3.Elapsed + ", chosen feature's id: " + chosenFeature.id + ", delete suggestions for this chosen feature");

                    foreach (string friend in friends)
                    {
                        Stopwatch w4 = Stopwatch.StartNew();
                        IEnumerable<User> friendInSocialTfs = db.Registrations.Where(r => r.idOnService == friend &&
                            r.serviceInstance == chosenFeature.serviceInstance).Select(r => r.User);
                        w4.Stop();
                        ILog log4 = LogManager.GetLogger("QueryLogger");
                        log4.Info(" Elapsed time: " + w4.Elapsed + ", user id: " + friend + ", feature's name: " + chosenFeature.serviceInstance + ", select all users that can be possible friends in SocialTFS");
                        if (friendInSocialTfs.Count() == 1)
                        {
                            User suggestedFriend = friendInSocialTfs.First();

                            if (friend != chosenFeature.Registration.idOnService)
                            {
                                Stopwatch w5 = Stopwatch.StartNew();
                                db.Suggestions.InsertOnSubmit(new Suggestion()
                                {
                                    user = suggestedFriend.id,
                                    chosenFeature = chosenFeature.id
                                });
                                w5.Stop();
                                ILog log5 = LogManager.GetLogger("QueryLogger");
                                log5.Info(" Elapsed time: " + w5.Elapsed + ", user id: " + suggestedFriend.id + ", chosen feature: " + chosenFeature.id + ", insert a suggestion in a pending state");

                                if (!logFriends.ContainsKey(suggestedFriend.id))
                                    logFriends[suggestedFriend.id] = new HashSet<int>();
                                logFriends[suggestedFriend.id].Add(temp.Registration.serviceInstance);
                            }
                        }
                    }
                    try
                    {
                        Stopwatch w6 = Stopwatch.StartNew();
                        db.SubmitChanges();
                        w6.Stop();
                        ILog log6 = LogManager.GetLogger("QueryLogger");
                        log6.Info(" Elapsed time: " + w6.Elapsed + ", insert a suggestion");
                    }
                    catch { }
                }
            }

            if (needToLog)
            {
                ILog log7 = LogManager.GetLogger("NetworkLogger");
                log7.Info(user.id + ",S,[" + GetFriendString(user.id, logFriends) + "]");
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
                Stopwatch w1 = Stopwatch.StartNew();
                owner = db.Users.Where(u => u.username == ownerName).Single();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", username: " + ownerName + ", select the username of the user to get his skills");
            }
            catch (Exception)
            {
                return new string[0];
            }

            DownloadSkills(owner, db);

            //get the names of the skills from the database
            Stopwatch w2 = Stopwatch.StartNew();
            IEnumerable<string> skills = db.Skills.Where(s => s.ChosenFeature.user == owner.id).Select(s => s.skill);
            w2.Stop();
            ILog log2 = LogManager.GetLogger("QueryLogger");
            log2.Info(" Elapsed time: " + w2.Elapsed + ", user id: " + owner.id + ", select all skills of an user");

            return skills.Distinct().ToArray<string>();
        }

        private void DownloadSkills(User currentUser, ConnectorDataContext db)
        {
            Stopwatch w = Stopwatch.StartNew();
            List<ChosenFeature> chosenFeatures = db.ChosenFeatures.Where(cf => cf.user == currentUser.id &&
                cf.feature == FeaturesType.Skills.ToString() &&
                cf.lastDownload < DateTime.UtcNow - _skillSpan).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + currentUser.id + ", feature's name: " + FeaturesType.Skills.ToString() + ", select all chosen features of an user and his feature 'skills'");

            foreach (ChosenFeature item in chosenFeatures)
            {
                //delete the user's skills in the database
                Stopwatch w1 = Stopwatch.StartNew();
                db.Skills.DeleteAllOnSubmit(item.Skills);
                db.SubmitChanges();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", delete user's skills");

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
                    Stopwatch w2 = Stopwatch.StartNew();
                    db.Skills.InsertOnSubmit(new Skill()
                    {
                        chosenFeature = item.id,
                        skill = userSkill
                    });
                    w2.Stop();
                    ILog log2 = LogManager.GetLogger("QueryLogger");
                    log2.Info(" Elapsed time: " + w2.Elapsed + ", chosen feature: " + item.id + ", skill: " + userSkill + ", preparing to insert skills");
                }

                item.lastDownload = DateTime.UtcNow;

                Stopwatch w3 = Stopwatch.StartNew();
                db.SubmitChanges();
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", actually inserting skills");
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
            Stopwatch w4 = Stopwatch.StartNew();
            IEnumerable<ChosenFeature> chosenFeaturesToDelete = db.ChosenFeatures.Where(c => c.user == user.id
                && !chosenFeatures.Contains(c.feature) && c.serviceInstance == serviceInstanceId);
            w4.Stop();
            ILog log4 = LogManager.GetLogger("QueryLogger");
            log4.Info(" Elapsed time: " + w4.Elapsed + ", user id: " + user.id + ", service instance: " + serviceInstanceId + ", select all chosen features of an user");
            Stopwatch w = Stopwatch.StartNew();
            db.ChosenFeatures.DeleteAllOnSubmit(chosenFeaturesToDelete);
            db.SubmitChanges();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", remove the old chosen features");

            //add the new chosen features
            foreach (string chosenFeature in chosenFeatures)
            {
                Stopwatch w1 = Stopwatch.StartNew();
                bool cFeature = db.ChosenFeatures.Where(c => c.user == user.id && c.feature == chosenFeature && c.serviceInstance == serviceInstanceId).Any();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", feature's name: " + chosenFeature + ", service instance's id: " + serviceInstanceId + ", check if there is a chosen feature with these parameters");
                if (!cFeature)
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
                    Stopwatch w2 = Stopwatch.StartNew();
                    db.ChosenFeatures.InsertOnSubmit(new ChosenFeature()
                    {
                        user = user.id,
                        serviceInstance = serviceInstanceId,
                        feature = chosenFeature,
                        lastDownload = new DateTime(1900, 1, 1)
                    });
                    w2.Stop();
                    ILog log2 = LogManager.GetLogger("QueryLogger");
                    log2.Info(" Elapsed time: " + w2.Elapsed + ", user id: " + user.id + ", service instance's id: " + serviceInstanceId + ", feature: " + chosenFeature + ", last download: " + new DateTime(1900, 1, 1) + ", insert a new chosen feature in a pending state");
                }
            }
            Stopwatch w3 = Stopwatch.StartNew();
            db.SubmitChanges();
            w3.Stop();
            ILog log3 = LogManager.GetLogger("QueryLogger");
            log3.Info(" Elapsed time: " + w3.Elapsed + ", insert the chosen feature");

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

            Stopwatch w1 = Stopwatch.StartNew();
            ServiceInstance sInstance = db.ServiceInstances.Where(si => si.id == serviceInstanceId).Single();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", service instance's id: " + serviceInstanceId + ", select service instance to get the chosen features");

            foreach (FeaturesType item in ServiceFactory.getService(sInstance.Service.name).GetPublicFeatures())
            {
                Stopwatch w2 = Stopwatch.StartNew();
                bool chosen = db.ChosenFeatures.Where(cf => cf.serviceInstance == serviceInstanceId && cf.user == user.id).Select(cf => cf.feature).Contains(item.ToString());
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", service istance's id: " + serviceInstanceId + ", user id: " + user.id + ", check if a chosen feature has been chosen by an user");
                WFeature feature = new WFeature()
                {
                    Name = item.ToString(),
                    Description = FeaturesManager.GetFeatureDescription(item),
                    IsChosen = chosen
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

            Stopwatch w1 = Stopwatch.StartNew();
            List<User> usr = db.Hiddens.Where(h => h.user == user.id).Select(h => h.Friend).Distinct().ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", select all users hidden by an user");

            foreach (User item in usr)
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

            Stopwatch w = Stopwatch.StartNew();
            List<Hidden> hide = db.Hiddens.Where(h => h.user == user.id && h.friend == userId).ToList();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", friend's id: " + userId + ", select all hidden friends of an user and set the visibility according to the timeline");

            foreach (Hidden item in hide)
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
                Stopwatch w = Stopwatch.StartNew();
                db.Hiddens.DeleteAllOnSubmit(db.Hiddens.Where(h => h.user == user.id && h.friend == userId));
                db.SubmitChanges();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", friend's id: " + userId + ", remove all hidden friends of an user");

                if (suggestions)
                {
                    Stopwatch w2 = Stopwatch.StartNew();
                    db.Hiddens.InsertOnSubmit(new Hidden()
                    {
                        user = user.id,
                        friend = userId,
                        timeline = HiddenType.Suggestions.ToString()
                    });
                    w2.Stop();
                    ILog log2 = LogManager.GetLogger("QueryLogger");
                    log2.Info(" Elapsed time: " + w2.Elapsed + ", user id: " + user.id + ", friend's id: " + userId + ", timeline: " + HiddenType.Suggestions.ToString() + ", insert a hidden friend in the suggestion timeline in a pending state");
                }
                if (dynamic)
                {
                    Stopwatch w3 = Stopwatch.StartNew();
                    db.Hiddens.InsertOnSubmit(new Hidden()
                    {
                        user = user.id,
                        friend = userId,
                        timeline = HiddenType.Dynamic.ToString()
                    });
                    w3.Stop();
                    ILog log3 = LogManager.GetLogger("QueryLogger");
                    log3.Info(" Elapsed time: " + w3.Elapsed + ", user id: " + user.id + ", friend's id: " + userId + ", timeline: " + HiddenType.Dynamic.ToString() + ", insert a hidden friend in the dynamic timeline in a pending state");
                }
                if (interactive)
                {
                    Stopwatch w4 = Stopwatch.StartNew();
                    db.Hiddens.InsertOnSubmit(new Hidden()
                    {
                        user = user.id,
                        friend = userId,
                        timeline = HiddenType.Interactive.ToString()
                    });
                    w4.Stop();
                    ILog log4 = LogManager.GetLogger("QueryLogger");
                    log4.Info(" Elapsed time: " + w4.Elapsed + ", user id: " + user.id + ", friend's id: " + userId + ", timeline: " + HiddenType.Interactive.ToString() + ", insert a hidden friend in the interactive timeline in a pending state");
                }
                Stopwatch w5 = Stopwatch.StartNew();
                db.SubmitChanges();
                w5.Stop();
                ILog log5 = LogManager.GetLogger("QueryLogger");
                log5.Info(" Elapsed time: " + w5.Elapsed + ", insert a hidden friend according to the timeline");
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
            Stopwatch w1 = Stopwatch.StartNew();
            List<ChosenFeature> cFeatures = db.ChosenFeatures.Where(cf => cf.user == user.id && cf.feature == FeaturesType.Avatar.ToString()).ToList();
            w1.Stop();
            ILog log1 = LogManager.GetLogger("QueryLogger");
            log1.Info(" Elapsed time: " + w1.Elapsed + ", user id: " + user.id + ", feature's name: " + FeaturesType.Avatar.ToString() + ", select all user's avatars");
            foreach (ChosenFeature chosenFeature in cFeatures)
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

            Stopwatch w = Stopwatch.StartNew();
            user.avatar = avatar.AbsoluteUri;
            db.SubmitChanges();
            w.Stop();
            ILog log = LogManager.GetLogger("QueryLogger");
            log.Info(" Elapsed time: " + w.Elapsed + ", uri: " + avatar.AbsoluteUri + ", save avatar");

            return true;
        }
    }
}
