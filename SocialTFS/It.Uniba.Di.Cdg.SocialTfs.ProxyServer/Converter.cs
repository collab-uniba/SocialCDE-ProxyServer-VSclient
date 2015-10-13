using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer
{
    /// <summary>
    /// Provides a set of type conversion between the types used for the database and the types used for the web.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Convert an User (used for the database) in a WUser (used for the web).
        /// </summary>
        /// <param name="db">Database connector data context.</param>
        /// <param name="user">User that requires the conversion.</param>
        /// <param name="userToConvert">The User to convert.</param>
        /// <param name="calculateInfos">True if you need to have all the information about the User, false otherwise.</param>
        /// <returns>A WUser.</returns>
        public static WUser UserToWUser(ConnectorDataContext db, User user, User userToConvert, bool calculateInfos)
        {
            WUser result = null;

            if (calculateInfos)
            {
                Stopwatch w = Stopwatch.StartNew();
                int stat = db.Posts.Where(p => p.ChosenFeature.user == userToConvert.id).Count();
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + userToConvert.id + ", count the number of posts of an user for a certain chosen feature");
                Stopwatch w1 = Stopwatch.StartNew();
                int followings = db.StaticFriends.Where(sf => sf.User == userToConvert).Count();
                w1.Stop();
                ILog log1 = LogManager.GetLogger("QueryLogger");
                log1.Info(" Elapsed time: " + w1.Elapsed + ", count the number of static friends of an user");
                Stopwatch w2 = Stopwatch.StartNew();
                int followers = db.StaticFriends.Where(sf => sf.Friend == userToConvert).Count();
                w2.Stop();
                ILog log2 = LogManager.GetLogger("QueryLogger");
                log2.Info(" Elapsed time: " + w2.Elapsed + ", count the number of users that are static friends of an user");
                Stopwatch w3 = Stopwatch.StartNew();
                int followed = db.StaticFriends.Where(sf => sf.User == user && sf.Friend == userToConvert).Count();
                w3.Stop();
                ILog log3 = LogManager.GetLogger("QueryLogger");
                log3.Info(" Elapsed time: " + w3.Elapsed + ", count the number of users that follow and are followed by an user");
                result = new WUser()
                {
                    Id = userToConvert.id,
                    Username = userToConvert.username,
                    Email = userToConvert.email,
                    Avatar = userToConvert.avatar,
                    Statuses = stat,
                    Followings = followings,
                    Followers = followers,
                    Followed = followed == 1
                };
            }
            else
            {
                result = new WUser()
                {
                    Id = userToConvert.id,
                    Username = userToConvert.username,
                    Email = userToConvert.email,
                    Avatar = userToConvert.avatar,
                    Statuses = -1,
                    Followings = -1,
                    Followers = -1,
                    Followed = false
                };
            }

            return result;
        }

        /// <summary>
        /// Convert a Post (used for the database) in a WPost (used for the web).
        /// </summary>
        /// <param name="db">Database connector data context.</param>
        /// <param name="user">User that requires the conversion.</param>
        /// <param name="post">The Post to convert.</param>
        /// <returns>A WPost.</returns>
        public static WPost PostToWPost(ConnectorDataContext db, User user, Post post)
        {
            WUser author = Converter.UserToWUser(db, user, post.ChosenFeature.Registration.User, false);

            WService service = Converter.ServiceInstanceToWService(db, user, post.ChosenFeature.Registration.ServiceInstance, false);

            WPost result = new WPost()
            {
                Id = post.id,
                User = author,
                Service = service,
                Message = post.message,
                CreateAt = post.createAt
            };

            return result;
        }

        /// <summary>
        /// Convert a ServiceInstance (used for the database) in a WService (used for the web).
        /// </summary>
        /// <param name="db">Database connector data context.</param>
        /// <param name="user">User that requires the conversion.</param>
        /// <param name="serviceInstance">The ServiceInstance to convert.</param>
        /// <param name="calculateFeature">True if you need to have all the information about the User, false otherwise.</param>
        /// <returns>A WService.</returns>
        public static WService ServiceInstanceToWService(ConnectorDataContext db, User user, ServiceInstance serviceInstance, bool calculateFeature)
        {
            WService result = null;

            if (calculateFeature)
            {
                bool isRegistered = false;
                Stopwatch w = Stopwatch.StartNew();
                IEnumerable<ServiceInstance> myServices = db.Registrations.Where(r => r.user == user.id).Select(r => r.ServiceInstance);
                w.Stop();
                ILog log = LogManager.GetLogger("QueryLogger");
                log.Info(" Elapsed time: " + w.Elapsed + ", user id: " + user.id + ", select all service instances of an user");
                if (myServices.Contains(serviceInstance))
                    isRegistered = true;

                List<FeaturesType> privateFeatures = ServiceFactory.getService(serviceInstance.Service.name).GetPrivateFeatures();
                bool requireOAuth = false;
                int oauthVersion = 0;
                if (privateFeatures.Contains(FeaturesType.OAuth1))
                {
                    requireOAuth = true;
                    oauthVersion = 1;
                }
                else if (privateFeatures.Contains(FeaturesType.OAuth2))
                {
                    requireOAuth = true;
                    oauthVersion = 2;
                }

                bool requireTFSAuthentication = false;
                bool requireTFSDomain = false;
                if (privateFeatures.Contains(FeaturesType.TFSAuthenticationWithDomain))
                {
                    requireTFSAuthentication = true;
                    requireTFSDomain = true;
                }
                else if (privateFeatures.Contains(FeaturesType.TFSAuthenticationWithoutDomain))
                {
                    requireTFSAuthentication = true;
                    requireTFSDomain = false;
                }

                result = new WService()
                {
                    Id = serviceInstance.id,
                    Name = serviceInstance.name,
                    Host = serviceInstance.host,
                    BaseService = serviceInstance.Service.name,
                    Image = serviceInstance.Service.image,
                    Registered = isRegistered,
                    RequireOAuth = requireOAuth,
                    OAuthVersion = oauthVersion,
                    RequireTFSAuthentication = requireTFSAuthentication,
                    RequireTFSDomain = requireTFSDomain
                };
            }
            else
            {
                result = new WService()
                {
                    Id = serviceInstance.id,
                    Name = serviceInstance.name,
                    BaseService = serviceInstance.Service.name,
                    Image = serviceInstance.Service.image
                };
            }

            return result;
        }
    }
}