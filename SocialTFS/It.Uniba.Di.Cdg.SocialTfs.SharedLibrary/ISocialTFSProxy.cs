using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// This interface rapresent the manager of all functions of the Proxy web service.
    /// </summary>
    /// <remarks>
    /// Lists all methods available on the web via REST requests
    /// to query or modify the services database.
    /// </remarks>
    [ServiceContract]
    public interface ISocialTFSProxy
    {

        /// <summary>
        /// Check the state of the web service.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a GET request "&lt;ServiceHost&gt;/IsWebSerciceRunning"
        /// and returns a JSON response.
        /// </remarks>
        /// <returns>True.</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "IsWebServiceRunning")]
        bool IsWebServiceRunning();

        /// <summary>
        /// Check that the username has not been assigned to another user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a GET request "&lt;ServiceHost&gt;/IsAvailable?username=username"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <returns>True if the username is not already assigned to another user, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "IsAvailable?username={username}")]
        bool IsAvailable(String username);

        /// <summary>
        /// Register a new user in the system.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/SubscribeUser"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="email">Email address to which the invitation was sent.</param>
        /// <param name="password">Password sent in the invitation.</param>
        /// <param name="username">New username chosen by the user.</param>
        /// <returns>
        /// 0 if subscription is successful,
        /// 1 if e-mail address does not exist in the database,
        /// 2 if password does not match with the e-mail address sent,
        /// 3 if username is already used by another user.
        /// </returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "SubscribeUser")]
        int SubscribeUser(String email, String password, String username);

        /// <summary>
        /// Change the user's password.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/ChangePassword"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="oldPassword">Password to change.</param>
        /// <param name="newPassword">Password replacement.</param>
        /// <returns>True if the change is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ChangePassword")]
        bool ChangePassword(String username, String oldPassword, String newPassword);

        /// <summary>
        /// Returns the data  of all aviable services.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetServices"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <returns>All aviable services.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetServices")]
        WService[] GetServices(String username, String password);

        /// <summary>
        /// Returns the profile of a user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a Post request "&lt;ServiceHost&gt;/GetUser"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <returns>The profile of the user.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetUser")]
        WUser GetUser(String username, String password);

        /// <summary>
        /// Returns the profile of another user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a Post request "&lt;ServiceHost&gt;/GetColleagueProfile"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="colleagueId">The id of the colleague.</param>
        /// <returns>The profile of the user.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetColleagueProfile")]
        WUser GetColleagueProfile(String username, String password, int colleagueId);

        /// <summary>
        /// Provides the data for OAuth version 1 authentication procedure.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetOAuthData"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="service">Identifier of the service.</param>
        /// <returns>The authorization link, the access token and the access secret.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetOAuth1Data")]
        WOAuthData GetOAuthData(String username, String password, int service);

        /// <summary>
        /// Finish the OAuth version 1 authentication procedure.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/AuthorizeOAuth1"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="service">Identifier of the service.</param>
        /// <param name="verifier">Verifier pin provided by the service.</param>
        /// <param name="accessToken">Access Token for the service instance.</param>
        /// <param name="accessToken">Access Secret for the service instance.</param>
        /// <returns>True if the authentication is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "Authorize")]
        bool Authorize(String username, String password, int service, String verifier, String accessToken, String accessSecret);

        /// <summary>
        /// Records a service without OAuth authetication procedure.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/RecordService"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="service">Identifier of the service.</param>
        /// <param name="usernameOnService">Username on the service.</param>
        /// <param name="passwordOnService">Password on the service.</param>
        /// <param name="domain">Domain on the service.</param>
        /// <returns>True if the registration is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "RecordService")]
        bool RecordService(String username, String password, int service, String usernameOnService, String passwordOnService, String domain);

        /// <summary>
        /// Delete the access data of the services to which the user is registered.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/DeleteRegistredService"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>True if the change to the database is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "DeleteRegistredService")]
        bool DeleteRegistredService(String username, String password, int service);

        /// <summary>
        /// Provides up to 20 static timeline posts.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetStaticTimeline"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="since">Id of last downloaded post.</param>
        /// <param name="to">Id of first downloaded post.</param>
        /// <returns>Static timeline.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetHomeTimeline")]
        WPost[] GetHomeTimeline(String username, String password, long since = 0, long to = 0);

        /// <summary>
        /// Provides up to 20 user's timeline posts.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetUserTimeline"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="ownerName">The owner of the timeline.</param>
        /// <param name="since">Id of last downloaded post.</param>
        /// <param name="to">Id of first downloaded post.</param>
        /// <returns>User timeline.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetUserTimeline")]
        WPost[] GetUserTimeline(String username, String password, String ownerName, long since = 0, long to = 0);

        /// <summary>
        /// Get the timeline built with iteration friends.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetIterationTimeline"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="since">Id of last downloaded post.</param>
        /// <param name="to">Id of first downloaded post.</param>
        /// <returns>Dynamic timeline.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetIterationTimeline")]
        WPost[] GetIterationTimeline(String username, String password, long since = 0, long to = 0);

        /// <summary>
        /// Get the timeline built with all people involved with the panels with which the user is interacting to.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetInteractiveTimeline"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="collection">Collection in which to try the interactive object reference.</param>
        /// <param name="interactiveObject">The interactive object around which to create the network.</param>
        /// <param name="interactiveObject">Array of all interacrive objects within user workspace.</param>
        /// <param name="since">Id of last downloaded post.</param>
        /// <param name="to">Id of first downloaded post.</param>
        /// <returns>Interactive timeline.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetInteractiveTimeline")]
        WPost[] GetInteractiveTimeline(string username, string password, string collection, string interactiveObject, string objectType, long since = 0, long to = 0);

        /// <summary>
        /// Publish a post user-generated.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/Post"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="message">Messagge of the post.</param>
        /// <returns>True if the publication of the post is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "Post")]
        bool Post(String username, String password, String message);

        /// <summary>
        /// Provides to a user to start following someone else.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/Follow"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="followId">Identifier of the user to follow.</param>
        /// <returns>True if the connection to the user is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "Follow")]
        bool Follow(String username, String password, int followId);

        /// <summary>
        /// Provides to a user to stop following someone else.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/Follow"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="followId">Identifier of the user to stop follow.</param>
        /// <returns>True if the disconnection to the user is successful, false otherwise.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "Unfollow")]
        bool Unfollow(String username, String password, int followId);

        /// <summary>
        /// Returns the users who the user follows.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetFollowings"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <returns>A list of following users.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetFollowings")]
        WUser[] GetFollowings(String username, String password);

        /// <summary>
        /// Returns the users who follow the user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetFollowers"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <returns>A list of follower users.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetFollowers")]
        WUser[] GetFollowers(String username, String password);

        /// <summary>
        /// Returns the friends of the user that are in the same time in SocialTfs and in an active microblog.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetSuggestedFriends"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <returns>A list of suggested friends.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetSuggestedFriends")]
        WUser[] GetSuggestedFriends(String username, String password);

        /// <summary>
        /// Returns the user's skills retrived from the social network that the user has actived
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetSkills"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="ownerName">The owner of the skills.</param>
        /// <returns>A list of suggested friends.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetSkills")]
        String[] GetSkills(String username, String password, String ownerName);

        /// <summary>
        /// Update the features that the user have chosen for a specific service instance.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/UpdateChosenFeatures"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="serviceInstanceId">The id of the service instance.</param>
        /// <param name="chosenFeatures">The list of the chosen features for the service instance.</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "UpdateChosenFeatures")]
        bool UpdateChosenFeatures(String username, String password, int serviceInstanceId, string[] chosenFeatures);

        /// <summary>
        /// Get all the users hidden by the current user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetHiddenUsers"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetHiddenUsers")]
        WUser[] GetHiddenUsers(String username, String password);

        /// <summary>
        /// Get the visibility of a user from the suggestions and automatic friendships of the current user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetUserHideSettings"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="user">User that current user want to get the visibility.</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetUserHideSettings")]
        WHidden GetUserHideSettings(String username, String password, int userId);

        /// <summary>
        /// Update the visibility of a user in the suggestions and automatic friendships for the current user.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/UpdateHiddenUser"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="user">User that current user want to update the visibility.</param>
        /// <param name="suggestions">True if current user want to hide user from suggestions, false otherwise.</param>
        /// <param name="dynamic">True if current user want to hide user from dynamic timeline, false otherwise.</param>
        /// <param name="interactive">True if current user want to hide user from interactive timeline, false otherwise.</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "UpdateHiddenUser")]
        bool UpdateHiddenUser(String username, String password, int userId, bool suggestions, bool dynamic, bool interactive);

        /// <summary>
        /// Returns the features that a user have chosen for a specific service instance.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetChosenFeatures"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <param name="serviceInstanceId">The id of the service instance.</param>
        /// <returns>A list of features.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetChosenFeatures")]
        WFeature[] GetChosenFeatures(String username, String password, int serviceInstanceId);

        /// <summary>
        /// Returns the available avatars that a user have chosen for a specific service instance.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/GetAvailableAvatars"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        /// <returns>A list of URI.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "GetAvailableAvatars")]
        Uri[] GetAvailableAvatars(String username, String password);

        /// <summary>
        /// Save the user's avatar.
        /// </summary>
        /// <remarks>
        /// It can be accessed by a POST request "&lt;ServiceHost&gt;/SaveAvatar"
        /// and returns a JSON response.
        /// </remarks>
        /// <param name="username">Name that identifies the user.</param>
        /// <param name="password">Password to check user identity.</param>
        [OperationContract]
        [WebInvoke(Method = "POST",
            BodyStyle = WebMessageBodyStyle.WrappedRequest,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "SaveAvatar")]
        bool SaveAvatar(String username, String password, Uri avatar);
    }
}
