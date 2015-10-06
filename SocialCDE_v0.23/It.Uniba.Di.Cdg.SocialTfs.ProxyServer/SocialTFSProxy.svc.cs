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
using System.Data.Objects;
using It.Uniba.Di.Cdg.SocialTfs.ProxyServer.Comparer;


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
        private TimeSpan _dynamicSpan = new TimeSpan(24, 0, 0);
        private TimeSpan _interactiveSpan = new TimeSpan(24, 0, 0);
        private TimeSpan _skillSpan = new TimeSpan(15, 0, 0, 0);
        private TimeSpan _educationSpan = new TimeSpan(15, 0, 0, 0);
        private TimeSpan _positionSpan = new TimeSpan(15, 0, 0, 0);
        private TimeSpan _reputationSpan = new TimeSpan(15, 0, 0, 0);

        /// <summary>
        /// This static constructor is called only one time, when the application is started. 
        /// It synchronizes the features available for each service with the features available in the database.
        /// </summary>
        static SocialTFSProxy()
        {
            SocialTFSEntities db = new SocialTFSEntities();
            //add the completely new features
            IEnumerable<FeaturesType> features = FeaturesManager.GetFeatures();
            foreach (FeaturesType featureType in features)
            {
                try
                {
                    String strFeatureType = featureType.ToString();
                    Feature fTest = db.Feature.FirstOrDefault<Feature>(f => f.pk_name == strFeatureType);

                    if (fTest == null)
                    {
                        db.Feature.AddObject(new Feature()
                        {
                            pk_name = featureType.ToString(),
                            description = FeaturesManager.GetFeatureDescription(featureType),
                            @public = FeaturesManager.IsPublicFeature(featureType)
                        });
                    }
                }
                catch (Exception)
                {
                }
            }
            db.SaveChanges();
        }

        public bool IsWebServiceRunning()
        {
            return true;
        }

        public bool IsAvailable(String username)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));

            SocialTFSEntities db = new SocialTFSEntities();

            try
            {
                User user = db.User.Where(u => u.username == username && u.active).Single();
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

            SocialTFSEntities db = new SocialTFSEntities();
            User user;
            try
            {
                user = db.User.Where(u => u.email == email).Single();
            }
            catch (InvalidOperationException)
            {
                return 1;
            }

            if (user.password != (password))
                return 2;

            if (!IsAvailable(username))
                return 3;

            user.username = username;
            user.active = true;

            List<ServiceInstance> tmplst = db.ServiceInstance.ToList<ServiceInstance>();
            ServiceInstance si = db.ServiceInstance.FirstOrDefault( _si => _si.Service.name == "SocialTFS");
            int pk_fk_serviceInstance = si.pk_id;

            Registration registration = new Registration()
            {
                User = user,
                pk_fk_serviceInstance = pk_fk_serviceInstance,
                nameOnService = username,
                idOnService = username
            };

            db.Registration.AddObject(registration);

            ChosenFeature cf = new ChosenFeature()
            {
                Registration = registration,
                fk_feature = FeaturesType.Post.ToString(),
                lastDownload = new DateTime(1900, 1, 1)
            };
            db.ChosenFeature.AddObject(cf);

            db.SaveChanges();

            return 0;
        }

        public bool ChangePassword(String username, String oldPassword, String newPassword)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(oldPassword));
            Contract.Requires(!String.IsNullOrEmpty(newPassword));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, oldPassword);
            if (user == null)
                return false;

            user.password = (newPassword);
            db.SaveChanges();
            return true;
        }

        public WService[] GetServices(String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WService[0];

            List<WService> result = new List<WService>();

            foreach (ServiceInstance item in db.ServiceInstance.Where(si => si.Service.name != "SocialTFS"))
            {
                result.Add(Converter.ServiceInstanceToWService(db, user, item, true));
            }
            return result.ToArray();
        }

        public WUser GetUser(String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);

            if (user == null)
                return null;

            return Converter.UserToWUser(db, user, user, true);
        }

        public WUser GetColleagueProfile(String username, String password, int colleagueId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);

            if (user == null)
                return null;

            User colleague = null;
            try
            {
                colleague = db.User.Where(u => u.pk_id == colleagueId).Single();
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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return null;

            ServiceInstance si = db.ServiceInstance.Where(s => s.pk_id == service).Single();

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

                if (si.Service.name.Equals("LinkedIn") || si.Service.name.Equals("StackOverflow"))
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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            ServiceInstance si = db.ServiceInstance.Where(s => s.pk_id == service).Single();
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


                if (si.Service.name.Equals("GitHub") || si.Service.name.Equals("LinkedIn") || si.Service.name.Equals("StackOverflow"))
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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            ServiceInstance serviceInstance = db.ServiceInstance.Where(s => s.pk_id == service).Single();

            IService iService = ServiceFactory.getService(
                serviceInstance.Service.name,
                usernameOnService,
                passwordOnService,
                domain,
                serviceInstance.host);

            IUser iUser = iService.VerifyCredential();

            //return RegisterUserOnAService(db, user, serviceInstance, iUser, db.EncDecRc4("key", passwordOnService), (String)iUser.Get(UserFeaturesType.Domain));
            return RegisterUserOnAService(db, user, serviceInstance, iUser, passwordOnService, (String)iUser.Get(UserFeaturesType.Domain));
        }

        private bool RegisterUserOnAService(SocialTFSEntities db, User user, ServiceInstance serviceInstance, IUser iUser, String accessToken, String accessSecret)
        {
            try
            {
                db.Registration.AddObject(new Registration
                {
                    pk_fk_user = user.pk_id,
                    ServiceInstance = serviceInstance,
                    nameOnService = iUser.UserName,
                    idOnService = iUser.Id.ToString(),
                    accessToken = accessToken,
                    accessSecret = accessSecret
                });

                db.SaveChanges();
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

            SocialTFSEntities db = new SocialTFSEntities();
            SocialTFSEntities db2 = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                var reg = db.Registration.Where(r => r.pk_fk_user == user.pk_id && r.pk_fk_serviceInstance == service);

                foreach(Registration r in reg)
                {
                    List<ChosenFeature> listaFeature = new List<ChosenFeature>();
                    foreach (ChosenFeature item in r.ChosenFeature)
                    {
                        listaFeature.Add(item);
                    }
                    foreach (ChosenFeature item in listaFeature)
                    {
                        //item è un join
                        //var row = db2.Reputation.FirstOrDefault(e => e.fk_chosenFeature == item.pk_id);

                        Reputation row = null;
                        List<ChosenFeature> listacf = db2.ChosenFeature.Where(c => c.fk_user == user.pk_id).ToList<ChosenFeature>();
                        foreach (ChosenFeature cf in listacf)
	                    {
		                    if ( cf.Reputation != null && cf.Reputation.Count > 0 )
                                row = cf.Reputation.FirstOrDefault<Reputation>();
	                    }

                        //Reputation row = item.Reputation.FirstOrDefault<Reputation>( r => user

                        if (row != null)
                        {
                            if (r.ServiceInstance.name == "Coderwall" && item.fk_feature == "Reputation")
                            {
                                row.coderwall_endorsements = null;
                                db2.SaveChanges();
                            }
                            else if (r.ServiceInstance.name == "Ohloh" && item.fk_feature == "Reputation")
                            {
                                row.ohloh_kudorank = null;
                                row.ohloh_kudoscore = null;
                                row.ohloh_bigCheese = null;
                                row.ohloh_fosser = null;
                                row.ohloh_orgMan = null;
                                row.ohloh_stacker = null;
                                db2.SaveChanges();
                            }

                            else if (r.ServiceInstance.name == "StackOverflow" && item.fk_feature == "Reputation")
                            {
                                row.stack_answer = null;
                                row.stack_question = null;
                                row.stack_reputationValue = null;
                                row.stack_bronze = null;
                                row.stack_silver = null;
                                row.stack_gold = null;
                                db2.SaveChanges();
                            }
                        }
                            
                        db.ChosenFeature.DeleteObject(item);
                        
                    }

                    db.Registration.DeleteObject(r);
                }

                db.SaveChanges();
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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            List<int> authors = db.StaticFriend.Where(f => f.fk_user == user.pk_id).Select(f => f.fk_friend).ToList();
            authors.Add(user.pk_id);

            return GetTimeline(db, user, authors, since, to);
        }

        public WPost[] GetUserTimeline(string username, string password, string ownerName, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            List<int> authors = new List<int> { db.User.Where(u => u.username == ownerName).Single().pk_id };

            return GetTimeline(db, user, authors, since, to);
        }

        public WPost[] GetIterationTimeline(string username, string password, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            String str = HiddenType.Dynamic.ToString();
            List<int> hiddenAuthors = db.Hidden.Where(h => h.fk_user == user.pk_id && h.timeline == str).Select(h => h.fk_friend).ToList();
            List<int> authors = db.DynamicFriend.Where(f => f.ChosenFeature.fk_user == user.pk_id && !hiddenAuthors.Contains(f.fk_user)).Select(f => f.fk_user).ToList();
            WPost[] timeline = GetTimeline(db, user, authors, since, to);

            new Thread(thread => UpdateDynamicFriend(user)).Start();

            return timeline;
        }

        private void UpdateDynamicFriend(User user)
        {
            SocialTFSEntities db = new SocialTFSEntities();

            String str = FeaturesType.IterationNetwork.ToString();
            foreach (ChosenFeature chosenFeature in db.ChosenFeature.Where(cf => cf.fk_user == user.pk_id && cf.fk_feature == str ))
            {
                ChosenFeature temp = db.ChosenFeature.Where(cf => cf.pk_id == chosenFeature.pk_id).Single();
                if (temp.lastDownload > DateTime.UtcNow - _dynamicSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try { db.SaveChanges(); }
                catch { continue; }

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
                        //db.EncDecRc4("key", temp.Registration.accessToken),
                        temp.Registration.accessToken,
                        temp.Registration.accessSecret,
                        temp.Registration.ServiceInstance.host);
                }
                //this line must be before the deleting
                String[] dynamicFriends = (String[])service.Get(FeaturesType.IterationNetwork, null);

                //delete old friendship for the current chosen feature
                var delFriends = db.DynamicFriend.Where(df => df.fk_chosenFeature == temp.pk_id);
                foreach (DynamicFriend s in delFriends)
                {
                    db.DynamicFriend.DeleteObject(s);
                }
                //db.DynamicFriend.DeleteAllOnSubmit(db.DynamicFrien.Where(df => df.chosenFeature == temp.pk_id));
                db.SaveChanges();

                foreach (String dynamicFriend in dynamicFriends)
                {
                    IEnumerable<int> friendsInDb = db.Registration.Where(r => r.nameOnService == dynamicFriend && r.pk_fk_serviceInstance == temp.fk_serviceInstance).Select(r => r.pk_fk_user);
                    foreach (int friendInDb in friendsInDb)
                    {
                        db.DynamicFriend.AddObject(new DynamicFriend()
                        {
                            fk_chosenFeature = temp.pk_id,
                            fk_user = friendInDb
                        });
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch { }
            }
        }

        public WPost[] GetInteractiveTimeline(string username, string password, string collectionUri, string interactiveObject, string objectType, long since, long to)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPost[0];

            if (String.IsNullOrEmpty(collectionUri))
            {
                collectionUri = FindGithubRepository(user, interactiveObject);
            }

            List<int> hiddenAuthors = db.Hidden.Where(h => h.fk_user == user.pk_id && h.timeline == HiddenType.Interactive.ToString()).Select(h => h.fk_friend).ToList();
            List<int> authors = db.InteractiveFriend.Where(f => f.ChosenFeature.fk_user == user.pk_id && f.collection == collectionUri && f.interactiveObject.EndsWith(interactiveObject) && f.objectType == objectType && !hiddenAuthors.Contains(f.fk_user)).Select(f => f.fk_user).ToList();
            WPost[] timeline = GetTimeline(db, user, authors, since, to);

            new Thread(thread => UpdateInteractiveFriend(user)).Start();

            return timeline;
        }

        private string FindGithubRepository(User user, string interactiveObject)
        {
            SocialTFSEntities db = new SocialTFSEntities();
            Boolean flag = false;
            IService service = null;

            foreach (ChosenFeature chosenFeature in db.ChosenFeature.Where(cf => cf.fk_user == user.pk_id && cf.fk_feature == FeaturesType.InteractiveNetwork.ToString()))
            {
                ChosenFeature temp = db.ChosenFeature.Where(cf => cf.pk_id == chosenFeature.pk_id).Single();

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

            SocialTFSEntities db = new SocialTFSEntities();
            String str = FeaturesType.InteractiveNetwork.ToString();

            foreach (ChosenFeature chosenFeature in db.ChosenFeature.Where(cf => cf.fk_user == user.pk_id && cf.fk_feature == str))
            {
                ChosenFeature temp = db.ChosenFeature.Where(cf => cf.pk_id == chosenFeature.pk_id).Single();
                if (temp.lastDownload > DateTime.UtcNow - _interactiveSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;

                try { db.SaveChanges(); }
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
                       //db.EncDecRc4("key", temp.Registration.accessToken),
                       temp.Registration.accessToken,
                       temp.Registration.accessSecret,
                       temp.Registration.ServiceInstance.host);
                }
                //this line must be before the deleting
                SCollection[] collections = (SCollection[])service.Get(FeaturesType.InteractiveNetwork);
                try
                {
                    //delete old friendship for the current chosen feature
                    System.Diagnostics.Debug.WriteLine(" id temp " + temp.pk_id);
                    System.Diagnostics.Debug.WriteLine(" db " + (db.InteractiveFriend == null));
                    System.Diagnostics.Debug.WriteLine(" collections length " + collections.Length);
                    /*
                    foreach (SCollection collection in collections)
                    {
                        System.Diagnostics.Debug.WriteLine(" collection " + collection.Uri);

                        foreach (SFile file in collection.Files)
                        {
                            System.Diagnostics.Debug.WriteLine(" file " + file.Name);

                            foreach (String utente in file.InvolvedUsers)
                            {
                                System.Diagnostics.Debug.WriteLine(" utente " + utente);
                            }
                        }

                    }
                    */
                    /*
                    SocialTFSEntities db2 = new SocialTFSEntities();
                    
                    IQueryable<InteractiveFriend> pr = db2.InteractiveFriends.Where(df => df.chosenFeature == temp.pk_id);

                    try
                    {
                        foreach (InteractiveFriend cf in pr)
                        {
                            System.Diagnostics.Debug.WriteLine(" id selezionato " + cf.pk_id);
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("seconda prova di integrità nulla");

                    }

                    IQueryable<InteractiveFriend> pr3 = db2.InteractiveFriends.Where(u => u.user == 3);
                    IQueryable<ChosenFeature> pr2 = db2.ChosenFeatures.Where(cl => cl.serviceInstance == 2);
                    //prova di integrità CHE FUNZIONA

                    System.Diagnostics.Debug.WriteLine(" righe selezionate chosen feature " + pr2.Count());

                    try
                    {
                        foreach (ChosenFeature cf in pr2)
                        {
                            System.Diagnostics.Debug.WriteLine(" id selezionato " + cf.pk_id);
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine(" prova di integrità nulla");
                    }
                    */
                    /*
                    
                    IQueryable<InteractiveFriend> pr3 = db2.InteractiveFriends.Where(u => u.user == 3);

                    //prova di integrità CHE FUNZIONA

                    System.Diagnostics.Debug.WriteLine(" righe selezionate chosen feature " + pr2.Count());

                    try
                    {
                        foreach (ChosenFeature cf in pr2)
                        {
                            System.Diagnostics.Debug.WriteLine(" id selezionato " + cf.pk_id);
                        }
                    }
                    catch 
                    {
                        System.Diagnostics.Debug.WriteLine(" prova di integrità nulla");
                    }

                    //prova di integrità 2 

                    try
                    {
                        foreach (InteractiveFriend  cf in pr3)
                        {
                            System.Diagnostics.Debug.WriteLine(" id selezionato " + cf.pk_id);
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("seconda prova di integrità nulla");
                        
                    }


                    System.Diagnostics.Debug.WriteLine(" righe selezionate " + pr.Count());

                    try
                    {
                        //1° tentativo
                       foreach(InteractiveFriend fr in pr)
                       {
                            db2.InteractiveFriends.DeleteOnSubmit(fr);
                        }
                    }
                    catch {

                        try
                        {
                            //2° tentativo
                            System.Diagnostics.Debug.WriteLine(" Enum fallito ");
                            List<InteractiveFriend> listfr = pr.ToList<InteractiveFriend>();
                            foreach (InteractiveFriend fr in pr)
                            {
                                db2.InteractiveFriends.DeleteOnSubmit(fr);
                            }
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine(" List fallito ");

                            try
                            {
                                //3° tentativo
                                var removeItems = (from c in db2.InteractiveFriends where c.chosenFeature == temp.pk_id select c);
                                foreach(var item in removeItems.AsEnumerable())
                                {
                                    db.InteractiveFriend.DeleteOnSubmit(item);
                                }
                            }
                            catch 
                            {
                                System.Diagnostics.Debug.WriteLine("Query statica fallita");
                                try
                                {
                                    InteractiveFriend[] fr = pr.ToArray<InteractiveFriend>();
                                    foreach (InteractiveFriend fr1 in pr)
                                    {
                                        db2.InteractiveFriends.DeleteOnSubmit(fr1);
                                    }
                                }
                                catch 
                                {
                                    System.Diagnostics.Debug.WriteLine(" Array Fallito");
                                }
                            }

                        }
                    }

                  
                    //ultimo tentativo
                    db2.InteractiveFriends.DeleteAllOnSubmit(pr.AsEnumerable<InteractiveFriend>()); 
                   //IEnumerable<InteractiveFriend> pr = (from c in db.InteractiveFriend.AsEnumerable()
                    // where c.chosenFeature == temp.pk_id select c).ToList();
                   
                   */


                    //vecchio metodo
                    var inFriends = db.InteractiveFriend.Where(df => df.fk_chosenFeature == temp.pk_id);
                    
                    foreach (InteractiveFriend i in inFriends)
                    {
                        db.InteractiveFriend.DeleteObject(i);
                    }
                    //db.InteractiveFriend.DeleteAllOnSubmit(db.InteractiveFriend.Where(df => df.fk_chosenFeature == temp.pk_id));
                    db.SaveChanges();

                    foreach (SCollection collection in collections)
                    {
                        foreach (SWorkItem workitem in collection.WorkItems)
                        {
                            IEnumerable<int> friendsInDb = db.Registration.Where(r => workitem.InvolvedUsers.Contains(r.nameOnService) || workitem.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.pk_fk_user);
                            foreach (int friendInDb in friendsInDb)
                                db.InteractiveFriend.AddObject(new InteractiveFriend()
                                {
                                    fk_user = friendInDb,
                                    fk_chosenFeature = temp.pk_id,
                                    collection = collection.Uri,
                                    interactiveObject = workitem.Name,
                                    objectType = "WorkItem"
                                });
                        }
                        foreach (SFile file in collection.Files)
                        {
                            IEnumerable<int> friendsInDb = db.Registration.Where(r => file.InvolvedUsers.Contains(r.nameOnService) || file.InvolvedUsers.Contains(r.accessSecret + "\\" + r.nameOnService)).Select(r => r.pk_fk_user);
                            foreach (int friendInDb in friendsInDb)
                                db.InteractiveFriend.AddObject(new InteractiveFriend()
                                {
                                    fk_user = friendInDb,
                                    fk_chosenFeature = temp.pk_id,
                                    collection = collection.Uri,
                                    interactiveObject = file.Name,
                                    objectType = "File"
                                });
                        }
                    }

                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }

        private WPost[] GetTimeline(SocialTFSEntities db, User user, List<int> authors, long since, long to)
        {
            List<WPost> result = new List<WPost>();

            try
            {
                if (since == 0 && to != 0)
                {
                    //checked if we have enought older posts
                    DateTime lastPostDate = db.Post.Where(tp => tp.pk_id == to).Single().createAt;
                    int olderPostCounter = db.Post.Where(p => authors.Contains(p.ChosenFeature.Registration.pk_fk_user) &&
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
                posts = db.Post.Where(p => authors.Contains(p.ChosenFeature.Registration.pk_fk_user)).OrderByDescending(p => p.createAt).Take(postLimit);
            else if (since != 0)
                posts = db.Post.Where(p => authors.Contains(p.ChosenFeature.Registration.pk_fk_user) && p.createAt > db.Post.Where(sp => sp.pk_id == since).FirstOrDefault().createAt).OrderByDescending(p => p.createAt).Take(postLimit);
            else
                posts = db.Post.Where(p => authors.Contains(p.ChosenFeature.Registration.pk_fk_user) && p.createAt < db.Post.Where(tp => tp.pk_id == to).FirstOrDefault().createAt).OrderByDescending(p => p.createAt).Take(postLimit);

            IEnumerable<int> followings = db.StaticFriend.Where(f => f.fk_user == user.pk_id).Select(f => f.fk_friend);

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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            int service = db.ServiceInstance.Where(si => si.Service.name == "SocialTFS").Single().pk_id;

            long chosenFeature = -1;

            try
            {
              String str = FeaturesType.Post.ToString();
              chosenFeature = db.ChosenFeature.Where(cf => cf.fk_user == user.pk_id && cf.fk_serviceInstance == service && cf.fk_feature == str).SingleOrDefault().pk_id;
            }
            catch (InvalidOperationException)
            {
                try
                {
                    db.Registration.Where(r => r.pk_fk_user == user.pk_id && r.pk_fk_serviceInstance == service).Single();
                }
                catch
                {
                    Registration registration = new Registration()
                    {
                        User = user,
                        pk_fk_serviceInstance = db.ServiceInstance.Where(si => si.Service.name == "SocialTFS").Single().pk_id,
                        nameOnService = username,
                        idOnService = username
                    };
                    db.Registration.AddObject(registration);
                    db.SaveChanges();
                }

                ChosenFeature newChoseFeature = new ChosenFeature()
                {
                    Registration = db.Registration.Where(r => r.pk_fk_user == user.pk_id && r.pk_fk_serviceInstance == service).Single(),
                    fk_feature = FeaturesType.Post.ToString(),
                    lastDownload = new DateTime(1900, 1, 1)
                };

                db.ChosenFeature.AddObject(newChoseFeature);
                db.SaveChanges();
                chosenFeature = newChoseFeature.pk_id;
            }

            db.Post.AddObject( new Post
            {
                fk_chosenFeature = chosenFeature,
                message = message,
                createAt = DateTime.UtcNow
            });

            db.SaveChanges();

            return true;

        }

        private User CheckCredentials(SocialTFSEntities db, String username, String password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            try
            {
                User user = db.User.Where(u => u.username == username && u.password == (password) && u.active).Single();
                return user;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private void DownloadNewerPost(int author)
        {
            SocialTFSEntities db = new SocialTFSEntities();

            //You could try using Convert.ToString(c.Id) instead of just calling c.Id.ToString()

            String str = FeaturesType.UserTimeline.ToString();
            List<ChosenFeature> chosenFeatures = db.ChosenFeature.Where(cf =>
                cf.Registration.pk_fk_user == author &&
                cf.fk_feature == str).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                ChosenFeature cfTemp = db.ChosenFeature.Where(cf => cf.pk_id == item.pk_id).Single();
                if (cfTemp.lastDownload >= DateTime.UtcNow - _postSpan)
                    continue;
                else
                    cfTemp.lastDownload = DateTime.UtcNow;

                try { db.SaveChanges(); }
                catch { continue; }

                long sinceId;
                DateTime sinceDate = new DateTime();
                try
                {
                    Post sincePost = db.Post.Where(p => p.fk_chosenFeature == cfTemp.pk_id).OrderByDescending(p => p.createAt).First();
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
                    timeline = (IPost[])service.Get(FeaturesType.UserTimeline, long.Parse(cfTemp.Registration.idOnService), sinceId, sinceDate);
                }


                IEnumerable<long?> postInDb = db.Post.Where(p => p.fk_chosenFeature == item.pk_id).Select(p => p.idOnService);

                foreach (IPost post in timeline)
                {
                    if (!postInDb.Contains(post.Id))
                        db.Post.AddObject(new Post
                        {
                            fk_chosenFeature = cfTemp.pk_id,
                            idOnService = post.Id,
                            message = post.Text,
                            createAt = post.CreatedAt
                        });
                }
            }
            try { db.SaveChanges(); }
            catch { ; }

        }

        private void DownloadOlderPost(int author)
        {
            SocialTFSEntities db = new SocialTFSEntities();

            List<ChosenFeature> chosenFeatures = db.ChosenFeature.Where(cf =>
                cf.Registration.pk_fk_user == author &&
                cf.fk_feature == FeaturesType.UserTimeline.ToString()).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                long maxId;
                DateTime maxDate = new DateTime();
                try
                {
                    Post maxPost = db.Post.Where(p => p.fk_chosenFeature == item.pk_id).OrderBy(p => p.createAt).First();
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
                IEnumerable<long?> postInDb = db.Post.Where(p => p.fk_chosenFeature == item.pk_id).Select(p => p.idOnService);


                //mod here
                foreach (IPost post in timeline)
                {
                    if (!postInDb.Contains(post.Id))
                        db.Post.AddObject(new Post
                        {
                            fk_chosenFeature = item.pk_id,
                            idOnService = post.Id,
                            message = post.Text,
                            createAt = post.CreatedAt
                        });
                }
            }
            db.SaveChanges();
        }

        public bool Follow(string username, string password, int followId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                db.StaticFriend.AddObject(new StaticFriend()
                {
                    fk_user = user.pk_id,
                    fk_friend = followId
                });

                db.SaveChanges();

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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                StaticFriend friend = db.StaticFriend.Where(f => f.fk_user == user.pk_id && f.fk_friend == followId).Single();

                db.StaticFriend.DeleteObject(friend);
                db.SaveChanges();

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

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            List<WUser> users = new List<WUser>();

            foreach (StaticFriend item in db.StaticFriend.Where(sf => sf.User.pk_id == user.pk_id))
            {
                users.Add(Converter.UserToWUser(db, user, item.Friend, false));
            }

            return users.ToArray();
        }

        public WUser[] GetFollowers(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            List<WUser> users = new List<WUser>();
            IEnumerable<int> followings = db.StaticFriend.Where(f => f.fk_user == user.pk_id).Select(f => f.fk_friend);

            foreach (StaticFriend item in db.StaticFriend.Where(f => f.Friend.pk_id == user.pk_id))
            {
                users.Add(Converter.UserToWUser(db, user, item.User, false));
            }

            return users.ToArray();
        }

        public WUser[] GetSuggestedFriends(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            List<WUser> suggestedFriends = new List<WUser>();

            foreach (User item in db.User)
            {
                if (item.username != username && item.username != "admin")
                {
                    suggestedFriends.Add(Converter.UserToWUser(db, user, item, false));
                }
                
            }


            //Incompatibile con ENTITY FRAMEWORK
            // Provides all suggested user not in Hidden or in StaticFriend, 
            // ordered by the sum of scores

            //List<User> suggestion =
            //(from s in db.Suggestion
            // join fs in db.FeatureScore
            //     on new { s.chosenFeature.fk_feature, s.ChosenFeature.serviceInstance }
            //     equals new { fs.feature, fs.serviceInstance }
            // where s.ChosenFeature.user == user.pk_id &&
            //     !db.Hidden.Where(h => h.user == s.ChosenFeature.user && h.timeline == HiddenType.Suggestions.ToString()).Select(h => h.friend).Contains(s.user) &&
            //     !db.StaticFriend.Where(sf => sf.user == s.ChosenFeature.user).Select(sf => sf.friend).Contains(s.user)
            // let key = new { s.User }
            // group fs by key into friend
            // orderby friend.Sum(af => af.score) descending
            // select friend.Key.User).ToList();

            new Thread(thread => UpdateSuggestion(user)).Start();

            return suggestedFriends.ToArray();
        }

        private void UpdateSuggestion(User user)
        {
            SocialTFSEntities db = new SocialTFSEntities();

            String str1 = FeaturesType.Followings.ToString();
            String str2 = FeaturesType.Followers.ToString();
            String str3 = FeaturesType.TFSCollection.ToString();
            String str4 = FeaturesType.TFSTeamProject.ToString();
            IEnumerable<ChosenFeature> chosenFeatures = db.ChosenFeature.Where(
                cf => (cf.fk_feature.Equals(str1) ||
                    cf.fk_feature.Equals(str2) ||
                    cf.fk_feature.Equals(str3) ||
                    cf.fk_feature.Equals(str4)) && cf.fk_user == user.pk_id);

            foreach (ChosenFeature chosenFeature in chosenFeatures)
            {
                ChosenFeature temp = db.ChosenFeature.Where(cf => cf.pk_id == chosenFeature.pk_id).Single();
                if (temp.lastDownload > DateTime.UtcNow - _suggestionSpan)
                    continue;
                else
                    temp.lastDownload = DateTime.UtcNow;


                try { db.SaveChanges(); }
                catch { continue; }

                IService service = null;

                if (chosenFeature.fk_feature.Equals(FeaturesType.Followings.ToString()) ||
                    chosenFeature.fk_feature.Equals(FeaturesType.Followers.ToString()))
                    service = ServiceFactory.getServiceOauth(
                        chosenFeature.Registration.ServiceInstance.Service.name,
                        chosenFeature.Registration.ServiceInstance.host,
                        chosenFeature.Registration.ServiceInstance.consumerKey,
                        chosenFeature.Registration.ServiceInstance.consumerSecret,
                        chosenFeature.Registration.accessToken,
                        chosenFeature.Registration.accessSecret);
                else if (chosenFeature.fk_feature.Equals(FeaturesType.TFSCollection.ToString()) ||
                    chosenFeature.fk_feature.Equals(FeaturesType.TFSTeamProject.ToString()))
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
                            //db.EncDecRc4("key", chosenFeature.Registration.accessToken),
                            chosenFeature.Registration.accessToken,
                            chosenFeature.Registration.accessSecret,
                            chosenFeature.Registration.ServiceInstance.host);
                    }
                }

                string[] friends = null;

                if (chosenFeature.fk_feature.Equals(FeaturesType.Followings.ToString()))
                    friends = (string[])service.Get(FeaturesType.Followings, null);
                else if (chosenFeature.fk_feature.Equals(FeaturesType.Followers.ToString()))
                    friends = (string[])service.Get(FeaturesType.Followers, null);
                else if (chosenFeature.fk_feature.Equals(FeaturesType.TFSCollection.ToString()))
                    friends = (string[])service.Get(FeaturesType.TFSCollection, null);
                else if (chosenFeature.fk_feature.Equals(FeaturesType.TFSTeamProject.ToString()))
                    friends = (string[])service.Get(FeaturesType.TFSTeamProject, null);

                if (friends != null)
                {
                    //Delete suggestions for this chosen feature in the database
                    var sug = db.Suggestion.Where(s => s.fk_chosenFeature == chosenFeature.pk_id);
                    foreach (Suggestion s in sug)
                    {
                        db.Suggestion.DeleteObject(s);
                    }

                    //db.Suggestion.DeleteAllOnSubmit(db.Suggestion.Where(s => s.fk_chosenFeature == chosenFeature.pk_id));
                    db.SaveChanges();

                    foreach (string friend in friends)
                    {
                        IEnumerable<User> friendInSocialTfs = db.Registration.Where(r => r.idOnService == friend &&
                            r.pk_fk_serviceInstance == chosenFeature.fk_serviceInstance).Select(r => r.User);

                        if (friendInSocialTfs.Count() == 1)
                        {
                            User suggestedFriend = friendInSocialTfs.First();

                            if (friend != chosenFeature.Registration.idOnService)
                            {
                                db.Suggestion.AddObject(new Suggestion()
                                {
                                    fk_user = suggestedFriend.pk_id,
                                    fk_chosenFeature = chosenFeature.pk_id
                                });
                            }
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch { }
                }
            }
        }

        public WPos[] GetPositions(string username, string password, string ownerName)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WPos[0];

            User owner;

            try
            {
                owner = db.User.Where(u => u.username == ownerName).Single();
            }
            catch (Exception)
            {
                return new WPos[0];
            }

            DownloadPositions(owner, db);

            String str = FeaturesType.Positions.ToString();
            ChosenFeature chosenFeature = db.ChosenFeature.FirstOrDefault(cf => cf.fk_user == owner.pk_id &&
                cf.fk_feature == str);
            List<WPos> ris = new List<WPos>();
            foreach (var item in db.Positions)
            {
                if (item.fk_chosenFeature == chosenFeature.pk_id)
                {
                    ris.Add(new WPos()
                    {
                        name = item.name,
                        title = item.title,
                        posId = item.pk_id,
                        industry = item.industry
                    });
                }
            }

            return ris.ToArray<WPos>();
        }

        private void DownloadPositions(User currentUser, SocialTFSEntities db)
        {
            String str = FeaturesType.Positions.ToString();
            DateTime tempoLimite = DateTime.UtcNow - _positionSpan;
            List<ChosenFeature> chosenFeatures = db.ChosenFeature.Where(cf => cf.fk_user == currentUser.pk_id &&
                cf.fk_feature == str &&
                cf.lastDownload < tempoLimite).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                //delete the user's positions in the database
                //db.Position.DeleteAllOnSubmit(item.Positions);
                foreach (Positions p in item.Positions)
                {
                    db.Positions.DeleteObject(p);
                }

                db.SaveChanges();

                Registration registration = item.Registration;
                IService service = ServiceFactory.getServiceOauth(
                    registration.ServiceInstance.Service.name,
                    registration.ServiceInstance.host,
                    registration.ServiceInstance.consumerKey,
                    registration.ServiceInstance.consumerSecret,
                    registration.accessToken,
                    registration.accessSecret);


                IPos[] userPositions = (IPos[])service.Get(FeaturesType.Positions, null);
                //String[] poss = (String[])service.Get(FeaturesType.Positions, null);
                //poss = poss;

                try
                {
                    foreach (IPos userPosition in userPositions)
                    {
                        db.Positions.AddObject(new Positions()
                        {
                            fk_chosenFeature = item.pk_id,
                            pk_id = userPosition.posId,
                            title = userPosition.title,
                            name = userPosition.name,
                            industry = userPosition.industry
                        });
                    }
                }
                catch (Exception)
                {

                    return;
                }

                item.lastDownload = DateTime.UtcNow;

                db.SaveChanges();
            }
        }

        public WEdu[] GetEducations(string username, string password, string ownerName)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WEdu[0];

            User owner;

            try
            {
                owner = db.User.Where(u => u.username == ownerName).Single();
            }
            catch (Exception)
            {
                return new WEdu[0];
            }

            //Inserisce nel db le Edu
            DownloadEducations(owner, db);

            //FIXME prende tutte le tutple, ma deve prendere solo quelle dell'utente
            //attraverso la chiave esterna fk_chosenFeature
            String str = FeaturesType.Educations.ToString();
            ChosenFeature chosenFeature = db.ChosenFeature.FirstOrDefault(cf => cf.fk_user == owner.pk_id &&
                cf.fk_feature == str);
       
            List<WEdu> ris = new List<WEdu>();
            foreach (var item in db.Educations)
	        {
                if (item.fk_chosenFeature == chosenFeature.pk_id)
                {
                    ris.Add(new WEdu()
                    {
                        fieldOfStudy = item.fieldOfStudy,
                        eduId = item.pk_id,
                        schoolName = item.schoolName
                    });
                }
	        }

            return ris.ToArray<WEdu>();
        }

        private void DownloadEducations(User currentUser, SocialTFSEntities db)
        {
            String str = FeaturesType.Educations.ToString();
            DateTime tempoLimite = DateTime.UtcNow - _educationSpan;
            List<ChosenFeature> chosenFeatures = db.ChosenFeature.Where(cf => cf.fk_user == currentUser.pk_id &&
                cf.fk_feature == str &&
                cf.lastDownload < tempoLimite).ToList();

            foreach (ChosenFeature item in chosenFeatures)
            {
                foreach (Educations edu in item.Educations)
                {
                    db.Educations.DeleteObject(edu);
                }

                db.SaveChanges();

                Registration registration = item.Registration;
                IService service = ServiceFactory.getServiceOauth(
                    registration.ServiceInstance.Service.name,
                    registration.ServiceInstance.host,
                    registration.ServiceInstance.consumerKey,
                    registration.ServiceInstance.consumerSecret,
                    registration.accessToken,
                    registration.accessSecret);

                IEdu[] userEducations = (IEdu[])service.Get(FeaturesType.Educations, null);

               
                //insert educations in the database
                try
                {
                    foreach (IEdu userEducation in userEducations)
                    {
                        if (db.Educations.FirstOrDefault(e => e.pk_id == userEducation.eduId) == null)
                        {
                            db.Educations.AddObject(new Educations()
                            {
                                pk_id = userEducation.eduId,
                                fk_chosenFeature = item.pk_id,
                                fieldOfStudy = userEducation.fieldOfStudy,
                                schoolName = userEducation.schoolName
                            });
                        }
                    }
                }
                catch (Exception)
                {

                    return;
                }

                item.lastDownload = DateTime.UtcNow;

                db.SaveChanges();
            }
        }

        public string[] GetSkills(string username, string password, string ownerName)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new string[0];

            User owner;

            try
            {
                owner = db.User.Where(u => u.username == ownerName).Single();
            }
            catch (Exception)
            {
                return new string[0];
            }

            DownloadSkills(owner, db);

            //get the names of the skills from the database
            IEnumerable<string> skills = db.Skills.Where(s => s.ChosenFeature.fk_user == owner.pk_id).Select(s => s.pk_skill_name);

            return skills.Distinct().ToArray<string>();
        }

        private void DownloadSkills(User currentUser, SocialTFSEntities db)
        {
            String str = FeaturesType.Skills.ToString();
            DateTime tempoLimite = DateTime.UtcNow - _skillSpan;

            List<ChosenFeature> chosenFeatures = (from ChosenFeature cf in db.ChosenFeature
                                                  where cf.fk_user == currentUser.pk_id &&
                                                        cf.fk_feature == str && cf.lastDownload < tempoLimite
                                                  select cf).ToList<ChosenFeature>();
                              
            foreach (ChosenFeature item in chosenFeatures)
            {
                foreach (Skills s in item.Skills)
                {
                    db.Skills.DeleteObject(s);
                }

                db.SaveChanges();

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
                    db.Skills.AddObject(
                        new Skills()
                        {
                            pk_fk_chosenFeature = item.pk_id,
                            pk_skill_name = userSkill
                        }
                    );
                }

                item.lastDownload = DateTime.UtcNow;

                db.SaveChanges();
            }
        }

        public WReputation GetReputations(string username, string password, string ownerName)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WReputation();

            User owner;

            try
            {
                owner = db.User.Where(u => u.username == ownerName).Single();
            }
            catch (Exception)
            {
                return new WReputation();
            }

            //Inserisce nel db le Reputations
            DownloadReputations(owner, db);

            /*
            List<WReputation> ris = new List<WReputation>();
            foreach (var item in db.Reputation)
            {
             * */

            try
            {
                int IDUser = db.User.Where<User>(u => u.username == username).First<User>().pk_id;
                ChosenFeature cf = db.ChosenFeature.Where<ChosenFeature>(c => c.fk_user == IDUser && c.fk_feature == "Reputation").FirstOrDefault<ChosenFeature>();
                Reputation item = db.Reputation.Where<Reputation>(r => r.fk_chosenFeature == cf.pk_id).First<Reputation>();
                return new WReputation()
                {
                    reputationId = item.pk_id,
                    stackAnswer = item.stack_answer,
                    stackQuestion = item.stack_question,
                    stackBronze = item.stack_bronze,
                    stackSilver = item.stack_silver,
                    stackGold = item.stack_gold,
                    stackReputationValue = item.stack_reputationValue,
                    ohlohBigcheese = item.ohloh_bigCheese,
                    ohlohFosser = item.ohloh_fosser,
                    ohlohOrgman = item.ohloh_orgMan,
                    ohlohStacker = item.ohloh_stacker,
                    ohlohKudoRank = item.ohloh_kudorank,
                    ohlohKudoScore = item.ohloh_kudoscore,
                    coderwallEndorsements = item.coderwall_endorsements,
                    linkedinRecommendations = item.linkedin_recommendations,
                    linkedinRecommenders = item.linkedin_recommenders
                };
            }
            catch (Exception)
            {
                return null;
            }

            /*
            Reputation r = db.Reputation.Where<Reputation>(r => r.ChosenFeature

            var rep = from Reputation r in db.Reputation 
                      join ChosenFeature cf in db.Reputation on r.fk_chosenFeature equals cf.pk_id 
                      
                              

                WReputation rep = new WReputation()
                {
                    reputationId = item.pk_id,
                    stackAnswer = item.stack_answer,
                    stackQuestion = item.stack_question,
                    stackBronze = item.stack_bronze,
                    stackSilver = item.stack_silver,
                    stackGold = item.stack_gold,
                    stackReputationValue = item.stack_reputationValue,
                    ohlohBigcheese = item.ohloh_bigCheese,
                    ohlohFosser = item.ohloh_fosser,
                    ohlohOrgman = item.ohloh_orgMan,
                    ohlohStacker = item.ohloh_stacker,
                    ohlohKudoRank = item.ohloh_kudorank,
                    ohlohKudoScore = item.ohloh_kudoscore,
                    coderwallEndorsements = item.coderwall_endorsements,
                    linkedinRecommendations = item.linkedin_recommendations,
                    linkedinRecommenders = item.linkedin_recommenders
                });
            }

            return ris.FirstOrDefault();
             * */
        }

        private void DownloadReputations(User currentUser, SocialTFSEntities db)
        {
            try
            {
                String str = FeaturesType.Reputation.ToString();
                DateTime tempoLimite = DateTime.UtcNow - _reputationSpan;
                List<ChosenFeature> chosenFeatures = db.ChosenFeature.Where(cf => cf.fk_user == currentUser.pk_id &&
                    cf.fk_feature == str).ToList();

                //cf.lastDownload < tempoLimite

                foreach (ChosenFeature item in chosenFeatures)
                {
                    /*
                    foreach (Reputation repu in item.Reputation)
                    {
                        db.Reputation.DeleteObject(repu);
                    }
                    db.SaveChanges();
                     * */

                    #region Get servizio

                    Registration registration = item.Registration;
                    IService service;
                    String currentService = registration.ServiceInstance.name;
                    if (currentService == "Coderwall" || currentService == "Ohloh")
                    {
                        service = ServiceFactory.getService(
                        registration.ServiceInstance.Service.name,
                        registration.nameOnService,
                        registration.User.password,
                        "standard-domain",
                        registration.ServiceInstance.host);
                    }
                    else
                    {
                        service = ServiceFactory.getServiceOauth(
                        registration.ServiceInstance.Service.name,
                        registration.ServiceInstance.host,
                        registration.ServiceInstance.consumerKey,
                        registration.ServiceInstance.consumerSecret,
                        registration.accessToken,
                        registration.accessSecret);
                    }

                    #endregion Get servizio

                    IReputation userReputations = (IReputation)service.Get(FeaturesType.Reputation, null);

                    #region New Reputation

                    // Controllo se la reputazione di un dato servizio e di un dato utente
                    // è presente nel DB
                    // Se esiste, la modifico, se non esiste la creo

                    Reputation testReputation = db.Reputation.FirstOrDefault<Reputation>(
                        r => r.ChosenFeature.fk_user == currentUser.pk_id);                

                    //rep = db.Reputation.FirstOrDefault(e => e.pk_id == userReputations.reputationId);
                    
                    if (testReputation == null)
                    {
                        switch (currentService)
                        {
                            case "Coderwall":
                                db.Reputation.AddObject( new Reputation()
                                {
                                    fk_chosenFeature = item.pk_id,
                                    stack_answer = null,
                                    stack_question = null,
                                    stack_bronze = null,
                                    stack_silver = null,
                                    stack_gold = null,
                                    stack_reputationValue = null,
                                    ohloh_kudorank = null,
                                    ohloh_bigCheese = null,
                                    ohloh_fosser = null,
                                    ohloh_orgMan = null,
                                    ohloh_stacker = null,
                                    ohloh_kudoscore = null,
                                    coderwall_endorsements = userReputations.coderwallEndorsements,
                                    linkedin_recommendations = null,
                                    linkedin_recommenders = null
                                });
                                break;

                            case "Ohloh":
                                db.Reputation.AddObject(new Reputation()
                            {
                                
                                fk_chosenFeature = item.pk_id,
                                stack_answer = null,
                                stack_question = null,
                                stack_bronze = null,
                                stack_silver = null,
                                stack_gold = null,
                                stack_reputationValue = null,
                                ohloh_kudorank = userReputations.ohlohKudoRank,
                                ohloh_bigCheese = userReputations.ohlohBigcheese,
                                ohloh_fosser = userReputations.ohlohFosser,
                                ohloh_orgMan = userReputations.ohlohOrgman,
                                ohloh_stacker = userReputations.ohlohStacker,
                                ohloh_kudoscore = userReputations.ohlohKudoScore,
                                coderwall_endorsements = null,
                                linkedin_recommendations = null,
                                linkedin_recommenders = null
                            });
                            break;

                            case "StackOverflow":
                                Reputation r = new Reputation()
                                {
                                    fk_chosenFeature = item.pk_id,
                                    stack_answer = userReputations.stackAnswer,
                                    stack_question = userReputations.stackQuestion,
                                    stack_bronze = userReputations.stackBronze,
                                    stack_silver = userReputations.stackSilver,
                                    stack_gold = userReputations.stackGold,
                                    stack_reputationValue = userReputations.stackReputationValue,
                                    ohloh_kudorank = null,
                                    ohloh_bigCheese = null,
                                    ohloh_fosser = null,
                                    ohloh_orgMan = null,
                                    ohloh_stacker = null,
                                    ohloh_kudoscore = null,
                                    coderwall_endorsements = null,
                                    linkedin_recommendations = null,
                                    linkedin_recommenders = null
                                };
                                db.Reputation.AddObject(r);
                                break;
                        }

                        /*
                        db.Reputation.AddObject(new Reputation()
                        {
                            pk_id = userReputations.reputationId,
                            fk_chosenFeature = item.pk_id,
                            stack_answer = userReputations.stackAnswer,
                            stack_question = userReputations.stackQuestion,
                            stack_bronze = userReputations.stackBronze,
                            stack_silver = userReputations.stackSilver,
                            stack_gold = userReputations.stackGold,
                            stack_reputationValue = userReputations.stackReputationValue,
                            ohloh_kudorank = userReputations.ohlohKudoRank,
                            ohloh_bigCheese = userReputations.ohlohBigcheese,
                            ohloh_fosser = userReputations.ohlohFosser,
                            ohloh_orgMan = userReputations.ohlohOrgman,
                            ohloh_stacker = userReputations.ohlohStacker,
                            ohloh_kudoscore = userReputations.ohlohKudoScore,
                            coderwall_endorsements = userReputations.coderwallEndorsements,
                            linkedin_recommendations = userReputations.linkedinRecommendations,
                            linkedin_recommenders = userReputations.linkedinRecommenders
                        });
                         * */

                        db.SaveChanges();

                    }

                    #endregion New Reputation

                    else

                    #region edit reputation

                    {
                        //aggiornamento tupla
                        //var testReputation = db.Reputation.FirstOrDefault(e => e.pk_id == userReputations.reputationId);
                        if (registration.ServiceInstance.name == "Coderwall")
                        {
                            testReputation.coderwall_endorsements = userReputations.coderwallEndorsements;
                        }
                        else if (registration.ServiceInstance.name == "Ohloh")
                        {
                            testReputation.ohloh_kudorank = userReputations.ohlohKudoRank;
                            testReputation.ohloh_kudoscore = userReputations.ohlohKudoScore;
                            testReputation.ohloh_bigCheese = userReputations.ohlohBigcheese;
                            testReputation.ohloh_fosser = userReputations.ohlohFosser;
                            testReputation.ohloh_orgMan = userReputations.ohlohOrgman;
                            testReputation.ohloh_stacker = userReputations.ohlohStacker;
                        }
                        else if (registration.ServiceInstance.name == "StackOverflow")
                        {
                            testReputation.stack_answer = userReputations.stackAnswer;
                            testReputation.stack_question = userReputations.stackQuestion;
                            testReputation.stack_reputationValue = userReputations.stackReputationValue;
                            testReputation.stack_bronze = userReputations.stackBronze;
                            testReputation.stack_silver = userReputations.stackSilver;
                            testReputation.stack_gold = userReputations.stackGold;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        db.SaveChanges();
                    
                    }

                    #endregion edit reputation

                    item.lastDownload = DateTime.UtcNow;
                }

                
            }
            catch (Exception e)
            {
                return;
            }
        }

        public bool UpdateChosenFeatures(string username, string password, int serviceInstanceId, string[] chosenFeatures)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();
            bool suggestion = false, dynamic = false, interactive = false;

            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            //remove the old chosen features
            IEnumerable<ChosenFeature> chosenFeaturesToDelete = db.ChosenFeature.Where(c => c.fk_user == user.pk_id
                && !chosenFeatures.Contains(c.fk_feature) && c.fk_serviceInstance == serviceInstanceId);

            foreach (ChosenFeature c in chosenFeaturesToDelete)
            {
                db.ChosenFeature.DeleteObject(c);
            }
            //db.ChosenFeature.DeleteAllOnSubmit(chosenFeaturesToDelete);
            db.SaveChanges();

            //add the new chosen features
            foreach (string chosenFeature in chosenFeatures)
            {
                if (!db.ChosenFeature.Where(c => c.fk_user == user.pk_id && c.fk_feature == chosenFeature && c.fk_serviceInstance == serviceInstanceId).Any())
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
                    db.ChosenFeature.AddObject(new ChosenFeature()
                    {
                        fk_user = user.pk_id,
                        fk_serviceInstance = serviceInstanceId,
                        fk_feature = chosenFeature,
                        lastDownload = new DateTime(1900, 1, 1)
                    });
                }
            }
            db.SaveChanges();

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

            SocialTFSEntities db = new SocialTFSEntities();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WFeature[0];

            List<WFeature> result = new List<WFeature>();

            foreach (FeaturesType item in ServiceFactory.getService(
                db.ServiceInstance.Where(si => si.pk_id == serviceInstanceId).Single().Service.name).GetPublicFeatures())
            {
                WFeature feature = new WFeature()
                {
                    Name = item.ToString(),
                    Description = FeaturesManager.GetFeatureDescription(item),
                    IsChosen = db.ChosenFeature.Where(cf => cf.fk_serviceInstance == serviceInstanceId &&
                        cf.fk_user == user.pk_id).Select(cf => cf.fk_feature).Contains(item.ToString())
                };
                result.Add(feature);
            }

            return result.ToArray();
        }

        public WUser[] GetHiddenUsers(string username, string password)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new WUser[0];

            List<WUser> result = new List<WUser>();

            foreach (User item in db.Hidden.Where(h => h.fk_user == user.pk_id).Select(h => h.Friend).Distinct())
            {
                result.Add(Converter.UserToWUser(db, user, item, false));
            }

            return result.ToArray();
        }

        public WHidden GetUserHideSettings(string username, string password, int userId)
        {
            Contract.Requires(!String.IsNullOrEmpty(username));
            Contract.Requires(!String.IsNullOrEmpty(password));

            SocialTFSEntities db = new SocialTFSEntities();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return null;

            WHidden result = new WHidden();

            foreach (Hidden item in db.Hidden.Where(h => h.fk_user == user.pk_id && h.fk_friend == userId))
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

            SocialTFSEntities db = new SocialTFSEntities();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                var hid = db.Hidden.Where(h => h.fk_user == user.pk_id && h.fk_friend == userId);
                foreach (Hidden h in hid)
                {
                    db.Hidden.DeleteObject(h);
                }
                //db.Hidden.DeleteAllOnSubmit(db.Hidden.Where(h => h.fk_user == user.pk_id && h.fk_friend == userId));
                db.SaveChanges();

                if (suggestions)
                    db.Hidden.AddObject(new Hidden()
                    {
                        fk_user = user.pk_id,
                        fk_friend = userId,
                        timeline = HiddenType.Suggestions.ToString()
                    });

                if (dynamic)
                    db.Hidden.AddObject(new Hidden()
                    {
                        fk_user = user.pk_id,
                        fk_friend = userId,
                        timeline = HiddenType.Dynamic.ToString()
                    });

                if (interactive)
                    db.Hidden.AddObject(new Hidden()
                    {
                        fk_user = user.pk_id,
                        fk_friend = userId,
                        timeline = HiddenType.Interactive.ToString()
                    });

                db.SaveChanges();
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

            SocialTFSEntities db = new SocialTFSEntities();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return new Uri[0];

            List<Uri> avatars = new List<Uri>();
            String str = FeaturesType.Avatar.ToString();
            foreach (ChosenFeature chosenFeature in db.ChosenFeature.Where(cf => cf.fk_user == user.pk_id && cf.fk_feature == str))
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

            SocialTFSEntities db = new SocialTFSEntities();
            User user = CheckCredentials(db, username, password);
            if (user == null)
                return false;

            try
            {
                user.avatar = avatar.AbsoluteUri;
                db.SaveChanges();
            }
            catch (Exception)
            {

                
            }

            return true;
        }
    }
}
