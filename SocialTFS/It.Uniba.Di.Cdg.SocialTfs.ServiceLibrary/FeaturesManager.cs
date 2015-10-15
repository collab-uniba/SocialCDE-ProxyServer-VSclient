using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{
    /// <summary>
    /// All types of features.
    /// </summary>
    /// <remarks>
    /// For a description of each feature see FeaturesManager.GetFeatureDescription(FeaturesType).
    /// </remarks>
    public enum FeaturesType
    {
        Avatar,
        TFSAuthenticationWithDomain,
        TFSAuthenticationWithoutDomain,
        TFSCollection,
        TFSTeamProject,
        IterationNetwork,
        InteractiveNetwork,
        Followers,
        Followings,
        MoreInstance,
        OAuth1,
        OAuth2,
        Post,
        Skills,
        UserTimeline,
        UserTimelineOlderPosts,
        Repository,
        Labels,
        UsersIssuesInvolved,
    }

    /// <summary>
    /// Types of features used by IUser.
    /// </summary>
    public enum UserFeaturesType
    {
        Domain
    }

    /// <summary>
    /// Manage the features.
    /// </summary>
    public static class FeaturesManager
    {
        /// <summary>
        /// Get the desription of a feature.
        /// </summary>
        /// <remarks>This description is shown to the user throw the client.</remarks>
        /// <param name="feature">The feature.</param>
        /// <returns>The description.</returns>
        public static string GetFeatureDescription(FeaturesType feature)
        {
            string result = "There is no description for this feature";

            switch (feature)
            {
                case FeaturesType.Avatar:
                    result = "Show your avatar";
                    break;
                case FeaturesType.TFSCollection:
                    result = "Suggest people in your collection";
                    break;
                case FeaturesType.TFSTeamProject:
                    result = "Suggest people in your team project";
                    break;
                case FeaturesType.IterationNetwork:
                    result = "Build a iteration network of friends";
                    break;
                case FeaturesType.InteractiveNetwork:
                    result = "Build an interactive network of friends";
                    break;
                case FeaturesType.Followers:
                    result = "Access your followers";
                    break;
                case FeaturesType.Followings:
                    result = "Access your followings";
                    break;
                case FeaturesType.MoreInstance:
                    result = "More instances available";
                    break;
                case FeaturesType.OAuth1:
                    result = "OAuth version 1 authorization required";
                    break;
                case FeaturesType.OAuth2:
                    result = "OAuth version 2 authorization required";
                    break;
                case FeaturesType.Post:
                    result = "Post a message";
                    break;
                case FeaturesType.Skills:
                    result = "Show your skills";
                    break;
                case FeaturesType.TFSAuthenticationWithDomain:
                    result = "Team Foundation Server authentication required secifing custom domain";
                    break;
                case FeaturesType.TFSAuthenticationWithoutDomain:
                    result = "Team Foundation Server authentication required with default domain";
                    break;
                case FeaturesType.UserTimeline:
                    result = "Show your timeline";
                    break;
                case FeaturesType.UserTimelineOlderPosts:
                    result = "Show older posts from user timeline";
                    break;
                case FeaturesType.UsersIssuesInvolved:
                    result = "Show users involved in an issue";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Establishes if a feature is public.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>True if the feature is public, false otherwise.</returns>
        public static bool IsPublicFeature(FeaturesType feature)
        {
            bool result = false;

            switch (feature)
            {
                case FeaturesType.Avatar:
                    result = true;
                    break;
                case FeaturesType.TFSCollection:
                    result = true;
                    break;
                case FeaturesType.TFSTeamProject:
                    result = true;
                    break;
                case FeaturesType.IterationNetwork:
                    result = true;
                    break;
                case FeaturesType.InteractiveNetwork:
                    result = true;
                    break;
                case FeaturesType.Followers:
                    result = true;
                    break;
                case FeaturesType.Followings:
                    result = true;
                    break;
                case FeaturesType.MoreInstance:
                    result = false;
                    break;
                case FeaturesType.Skills:
                    result = true;
                    break;
                case FeaturesType.UserTimeline:
                    result = true;
                    break;
            }

            return result;
        }

        /// <summary>
        /// The the list of all feature defined by the enumerator FeaturesType.
        /// </summary>
        /// <returns>A list of feature.</returns>
        public static IEnumerable<FeaturesType> GetFeatures()
        {
            return Enum.GetValues(typeof(FeaturesType)).Cast<FeaturesType>();
        }
    }
}
