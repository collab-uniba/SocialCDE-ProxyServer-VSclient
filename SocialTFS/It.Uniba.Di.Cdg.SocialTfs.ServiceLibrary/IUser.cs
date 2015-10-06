using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{
    public interface IUser
    {
        /// <summary>
        /// Identifier number of the user.
        /// </summary>
        String Id { get; }

        /// <summary>
        /// User name.
        /// </summary>
        String UserName { get; }

        /// <summary>
        /// This method is a generic method that given a feature and an arbitrary array of parameters, returns a generic result.
        /// </summary>
        /// <remarks>
        /// This method facilitates the flexibility of the system. In this way, when a new feature is added for a single service (refers to the user's features), the interface IUser don't change and that you don't need to change the other implementation of users for each service.
        /// </remarks>
        /// <param name="feature">A feature.</param>
        /// <param name="param">An array of parameters.</param>
        /// <returns>The result.</returns>
        Object Get(UserFeaturesType feature, params Object[] param);
    }
}
