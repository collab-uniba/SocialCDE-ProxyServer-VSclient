using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of feature data via REST requests.
    /// </summary>
    [DataContract]
    public class WOAuthData
    {
        /// <summary>
        /// Link to authorization page of the service instance.
        /// </summary>
        [DataMember]
        public String AuthorizationLink { get; set; }

        /// <summary>
        /// Access Token of the service instance.
        /// </summary>
        [DataMember]
        public String AccessToken { get; set; }

        /// <summary>
        /// AccessSecret of the service instance.
        /// </summary>
        [DataMember]
        public String AccessSecret { get; set; }
    }
}