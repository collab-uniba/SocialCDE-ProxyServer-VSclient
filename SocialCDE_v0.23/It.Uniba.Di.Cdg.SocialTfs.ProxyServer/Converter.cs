using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary;

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
        public static WUser UserToWUser(SocialTFSEntities db, User user, User userToConvert, bool calculateInfos)
        {
            WUser result = new WUser();

            //result.Statuses = db.Post.Where( p => p.ChosenFeature.fk_user == userToConvert.pk_id ).Count();
            //result.Followings = db.StaticFriend.Where(sf => sf.fk_user == userToConvert.pk_id ).Count();
            //result.Followers = db.StaticFriend.Where(sf => sf.fk_friend == userToConvert.pk_id).Count();
            //result.Followed = db.StaticFriend.Where(sf => sf.fk_user == user.pk_id && sf.fk_friend == userToConvert.pk_id ).Count() == 1;

            if (calculateInfos)
            {
                result = new WUser()
                {
                    Id = userToConvert.pk_id,
                    Username = userToConvert.username,
                    Email = userToConvert.email,
                    Avatar = userToConvert.avatar,
                    Statuses = db.Post.Where( p => p.ChosenFeature.fk_user == userToConvert.pk_id ).Count(),
                    Followings = db.StaticFriend.Where(sf => sf.User.pk_id == userToConvert.pk_id).Count(),
                    Followers = db.StaticFriend.Where(sf => sf.Friend.pk_id == userToConvert.pk_id).Count(),
                    Followed = db.StaticFriend.Where(sf => sf.User.pk_id == user.pk_id && sf.Friend.pk_id == userToConvert.pk_id).Count() == 1
                };
            }
            else
            {
                result = new WUser()
                {
                    Id = userToConvert.pk_id,
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
        public static WPost PostToWPost(SocialTFSEntities db, User user, Post post)
        {
            WUser author = Converter.UserToWUser(db, user, post.ChosenFeature.Registration.User, false);

            WService service = Converter.ServiceInstanceToWService(db, user, post.ChosenFeature.Registration.ServiceInstance, false);

            WPost result = new WPost()
            {
                Id = post.pk_id,
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
        public static WService ServiceInstanceToWService(SocialTFSEntities db, User user, ServiceInstance serviceInstance, bool calculateFeature)
        {
            WService result = null;

            if (calculateFeature)
            {
                bool isRegistered = false;
                IEnumerable<ServiceInstance> myServices = db.Registration.Where(r => r.pk_fk_user == user.pk_id).Select(r => r.ServiceInstance);
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
                    Id = serviceInstance.pk_id,
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
                    Id = serviceInstance.pk_id,
                    Name = serviceInstance.name,
                    BaseService = serviceInstance.Service.name,
                    Image = serviceInstance.Service.image
                };
            }

            return result;
        }
    }
}