using System.ServiceModel;
using It.Uniba.Di.Cdg.SocialTfs.SharedLibrary;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Net;

namespace It.Uniba.Di.Cdg.SocialTfs.Client
{
    /// <summary>
    /// Throws this class it is possible to access to all methods offered by the SocialTFS Proxy Server.
    /// </summary>
    internal class SocialTFSProxyClient : ClientBase<ISocialTFSProxy>, ISocialTFSProxy
    {
        internal SocialTFSProxyClient(string address) : base(new WebHttpBinding(), new EndpointAddress(address))
        {
            this.Endpoint.Behaviors.Add(new System.ServiceModel.Description.WebHttpBehavior());
        }

        bool ISocialTFSProxy.IsWebServiceRunning()
        {
            try
            {
                return this.Channel.IsWebServiceRunning();
            }
            catch(Exception e)
            {
                if (UIController.Connected)
                    UIController.UiDispacher.BeginInvoke(new Action(delegate()
                    {
                        UIController.ShowLostConnection();
                    }));
                return false;
            }
        }

        bool ISocialTFSProxy.IsAvailable(string username)
        {
            try
            {
                return this.Channel.IsAvailable(username);
            }
            catch
            {
                return false;
            }
        }

        int ISocialTFSProxy.SubscribeUser(string email, string password, string username)
        {
            try
            {
                return Channel.SubscribeUser(email, password, username);
            }
            catch
            {
                return -1;
            }
        }

        bool ISocialTFSProxy.ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                return Channel.ChangePassword(username, oldPassword, newPassword);
            }
            catch
            {
                return false;
            }
        }

        WService[] ISocialTFSProxy.GetServices(string username, string password)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetServices(username, password);
                return new WService[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WService[0];
            }
        }

        WUser ISocialTFSProxy.GetUser(string username, string password)
        {
            try
            {
                return Channel.GetUser(username, password);
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return null;
            }
        }

        WUser ISocialTFSProxy.GetColleagueProfile(string username, string password, int colleagueId)
        {
            try
            {
                return Channel.GetColleagueProfile(username, password, colleagueId);
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return null;
            }
        }

        WOAuthData ISocialTFSProxy.GetOAuthData(string username, string password, int service)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetOAuthData(username, password, service);
                return null;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return null;
            }
        }

        bool ISocialTFSProxy.Authorize(string username, string password, int service, string verifier, string accessToken, string accessSecret)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.Authorize(username, password, service, verifier, accessToken, accessSecret);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        bool ISocialTFSProxy.RecordService(string username, string password, int service, string usernameOnService, String passwordOnService, String domain)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.RecordService(username, password, service, usernameOnService, passwordOnService, domain);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        bool ISocialTFSProxy.DeleteRegistredService(string username, string password, int service)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.DeleteRegistredService(username, password, service);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        WPost[] ISocialTFSProxy.GetHomeTimeline(string username, string password, long since, long to)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetHomeTimeline(username, password, since, to);
                return new WPost[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WPost[0];
            }
        }

        WPost[] ISocialTFSProxy.GetUserTimeline(string username, string password, string ownerName, long since, long to)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetUserTimeline(username, password, ownerName, since, to);
                return new WPost[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WPost[0];
            }
        }

        WPost[] ISocialTFSProxy.GetIterationTimeline(string username, string password, long since, long to)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetIterationTimeline(username, password, since, to);
                return new WPost[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WPost[0];
            }
        }

        WPost[] ISocialTFSProxy.GetInteractiveTimeline(string username, string password, string collection, string interactiveObject, string objectType, long since, long to)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetInteractiveTimeline(username, password, collection, interactiveObject, objectType, since, to);
                return new WPost[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WPost[0];
            }
        }

        bool ISocialTFSProxy.Post(string username, string password, string message)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.Post(username, password, message);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        bool ISocialTFSProxy.Follow(string username, string password, int followId)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.Follow(username, password, followId);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        bool ISocialTFSProxy.Unfollow(string username, string password, int followId)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.Unfollow(username, password, followId);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        WUser[] ISocialTFSProxy.GetFollowings(string username, string password)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetFollowings(username, password);
                return new WUser[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WUser[0];
            }
        }

        WUser[] ISocialTFSProxy.GetFollowers(string username, string password)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetFollowers(username, password);
                return new WUser[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WUser[0];
            }
        }

        WUser[] ISocialTFSProxy.GetSuggestedFriends(string username, string password)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetSuggestedFriends(username, password);
                return new WUser[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WUser[0];
            }
        }

        string[] ISocialTFSProxy.GetSkills(string username, string password, string ownerName)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetSkills(username, password, ownerName);
                return new string[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new string[0];
            }
        }

        bool ISocialTFSProxy.UpdateChosenFeatures(string username, string password, int serviceInstanceId, string[] chosenFeatures)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.UpdateChosenFeatures(username, password, serviceInstanceId, chosenFeatures);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }


        WFeature[] ISocialTFSProxy.GetChosenFeatures(string username, string password, int serviceInstanceId)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetChosenFeatures(username, password, serviceInstanceId);
                return new WFeature[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new WFeature[0];
            }
        }

        WUser[] ISocialTFSProxy.GetHiddenUsers(string username, string password)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetHiddenUsers(username, password);
                return null;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return null;
            }
        }

        WHidden ISocialTFSProxy.GetUserHideSettings(string username, string password, int userId)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetUserHideSettings(username, password, userId);
                return null;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return null;
            }
        }

        bool ISocialTFSProxy.UpdateHiddenUser(string username, string password, int userId, bool suggestions, bool dynamic, bool interactive)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.UpdateHiddenUser(username, password, userId,suggestions,dynamic,interactive);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }

        Uri[] ISocialTFSProxy.GetAvailableAvatars(string username, string password)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.GetAvailableAvatars(username, password);
                return new Uri[0];
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return new Uri[0];
            }
        }

        bool ISocialTFSProxy.SaveAvatar(string username, string password, Uri avatar)
        {
            try
            {
                if (UIController.Connected)
                    return Channel.SaveAvatar(username, password, avatar);
                return false;
            }
            catch
            {
                ((ISocialTFSProxy)this).IsWebServiceRunning();
                return false;
            }
        }
    }
}
