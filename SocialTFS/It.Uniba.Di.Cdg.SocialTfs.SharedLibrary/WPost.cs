using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of post data via REST requests.
    /// </summary>
    [DataContract]
    public class WPost
    {
        /// <summary>
        /// Identifier of the post.
        /// </summary>
        [DataMember]
        public long Id { get; set; }

        /// <summary>
        /// Name of the author of the post.
        /// </summary>
        [DataMember]
        public WUser User { get; set; }

        /// <summary>
        /// Name of the service.
        /// </summary>
        [DataMember]
        public WService Service { get; set; }

        /// <summary>
        /// Message of the post.
        /// </summary>
        [DataMember]
        public String Message { get; set; }

        /// <summary>
        /// Creation date of the post.
        /// </summary>
        [DataMember]
        public DateTime CreateAt { get; set; }
    }
}