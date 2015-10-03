using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.StatusNet;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Linkedin;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Twitter;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.TeamFoundationServer;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Facebook;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Yammer;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.CodePlex;
using It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.GitHub; 

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{
    public class ServiceFactory
    {
        public static string GitHubLabels = string.Empty;

        /// <summary>
        /// Provide a specific service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="host">Root path for API of the service.</param>
        /// <param name="consumerKey">Consumer key of the service.</param>
        /// <param name="consumerSecret">Consumer secret of the service.</param>
        /// <param name="accessToken">Access token of the service.</param>
        /// <param name="accessSecret">Access secret of the service.</param>
        /// <returns>A service</returns>
        public static IService getServiceOauth(String serviceName, String host, String consumerKey, String consumerSecret, String accessToken, String accessSecret)
        {
            Contract.Requires(!String.IsNullOrEmpty(serviceName));
            Contract.Ensures(Contract.Result<IService>() != null);

            switch (serviceName)
            {
                case "StatusNet":
                    return new StatusNetService(host, consumerKey, consumerSecret, accessToken, accessSecret);
                case "Twitter":
                    return new TwitterService(host, consumerKey, consumerSecret, accessToken, accessSecret);
                case "Yammer":
                    return new YammerService(host, consumerKey, consumerSecret, accessToken, accessSecret);
                case "LinkedIn":
                    return new LinkedInService(host, consumerKey, consumerSecret, accessToken);
                case "Facebook":
                    return new FacebookService(host, consumerKey, consumerSecret, accessToken);
                case "GitHub":
                    return new GitHubService(host,consumerKey,consumerSecret,accessToken);
                default:
                    throw new Exception("\"" + serviceName + "\" service does not exist");
            }
        }

        /// <summary>
        /// Provide a specific service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="host">Root path of the service.</param>
        /// <param name="id">Identifier of current user on the service.</param>
        /// <param name="username">Username of current user on the service.</param>
        /// <returns>A service</returns>
        public static IService getService(String serviceName, String username, String password, String domain, String host)
        {
            Contract.Requires(!String.IsNullOrEmpty(serviceName));
            Contract.Ensures(Contract.Result<IService>() != null);

            switch (serviceName)
            {
                case "Team Foundation Server":
                    return new TeamFoundationServerService(username, password, domain, host);
                case "CodePlex":
                    return new CodePlexService(username, password, host);
                default:
                    throw new Exception("\"" + serviceName + "\" service does not exist");
            }
        }

       

        /// <summary>
        /// Provide a specific service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <remarks>With the returned service is not possible to connect to the service host.</remarks>
        /// <returns>A service in offline mode.</returns>
        public static IService getService(String serviceName)
        {
            Contract.Requires(!String.IsNullOrEmpty(serviceName));

            switch (serviceName)
            {
                case "StatusNet":
                    return new StatusNetService();
                case "Twitter":
                    return new TwitterService();
                case "Yammer":
                    return new YammerService();
                case "LinkedIn":
                    return new LinkedInService();
                case "Facebook":
                    return new FacebookService();
                case "Team Foundation Server":
                    return new TeamFoundationServerService();
                case "CodePlex":
                    return new CodePlexService();
                case "GitHub":
                    return new GitHubService();
                default:
                    throw new Exception("\"" + serviceName + "\" service does not exist");
            }
        }
    }
}
