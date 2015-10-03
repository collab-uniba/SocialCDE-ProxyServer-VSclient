using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{

    public interface IService
    {
        /// <summary>
        /// The version of the service implementation.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// The name of the service.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Provide the current user's information in the service.
        /// </summary>
        /// <returns>Current user</returns>
        IUser VerifyCredential();

        /// <summary>
        /// Get the list of all public features.
        /// </summary>
        /// <returns>A list of features.</returns>
        List<FeaturesType> GetPublicFeatures();

        /// <summary>
        /// Get the list of all private features.
        /// </summary>
        /// <returns>A list of features.</returns>
        List<FeaturesType> GetPrivateFeatures();

        /// <summary>
        /// Get the list of all features that contribute to the calculation of the score for the specific service.
        /// </summary>
        /// <returns>A list of features.</returns>
        List<FeaturesType> GetScoredFeatures();

        /// <summary>
        /// This method is a generic method that given a feature and an arbitrary array of parameters, returns a generic result.
        /// </summary>
        /// <remarks>
        /// This method facilitates the flexibility of the system. In this way, when a new feature is added for a single service, the interface IService don't change and that you don't need to change the implementation of the other services.
        /// </remarks>
        /// <param name="feature">A feature.</param>
        /// <param name="param">An array of parameters.</param>
        /// <returns>The result.</returns>
        Object Get(FeaturesType feature, params Object[] param);
    
    }
}
