using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of user data via REST requests.
    /// </summary>
    [DataContract]
    public class WUser
    {
        /// <summary>
        /// Identifier of the user.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Identification name of the user.
        /// </summary>
        [DataMember]
        public String Username { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        [DataMember]
        public String Email { get; set; }

        /// <summary>
        /// Image avatar of the user.
        /// </summary>
        [DataMember]
        public String Avatar { get; set; }

        /// <summary>
        /// Number of statuses written by the user stored in the database.
        /// </summary>
        [DataMember]
        public int Statuses { get; set; }

        /// <summary>
        /// Number of followings of the user.
        /// </summary>
        [DataMember]
        public int Followings { get; set; }

        /// <summary>
        /// Number of followers of the user.
        /// </summary>
        [DataMember]
        public int Followers { get; set; }

        /// <summary>
        /// True if current user follow this user, false otherwise.
        /// </summary>
        [DataMember]
        public bool Followed { get; set; }
    }
}