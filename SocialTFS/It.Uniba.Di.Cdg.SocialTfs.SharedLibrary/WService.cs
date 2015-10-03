using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{

    /// <summary>
    /// A wrapper to allow the transmission of services data via REST requests.
    /// </summary>
    [DataContract]
    public class WService
    {
        /// <summary>
        /// Identifier of the service.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Name of the service.
        /// </summary>
        [DataMember]
        public String Name { get; set; }

        /// <summary>
        /// Host of the service.
        /// </summary>
        [DataMember]
        public String Host { get; set; }

        /// <summary>
        /// Service to the base of the service.
        /// </summary>
        [DataMember]
        public String BaseService { get; set; }

        /// <summary>
        /// Image logo of the service.
        /// </summary>
        [DataMember]
        public String Image { get; set; }

        /// <summary>
        /// True if the current user is registered to the service. False otherwise.
        /// </summary>
        [DataMember]
        public bool Registered { get; set; }

        /// <summary>
        /// True if the service require OAuth procedure, false otherwise.
        /// </summary>
        [DataMember]
        public bool RequireOAuth { get; set; }

        /// <summary>
        /// Version of OAuth procedure required.
        /// </summary>
        [DataMember]
        public int OAuthVersion { get; set; }

        /// <summary>
        /// True if the service require TFS authetication procedure, false otherwise.
        /// </summary>
        [DataMember]
        public bool RequireTFSAuthentication { get; set; }

        /// <summary>
        /// True if the TFS authetication procedure require domain, false otherwise.
        /// </summary>
        [DataMember]
        public bool RequireTFSDomain { get; set; }
    }
}