using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of edu data via REST requests.
    /// </summary>
    [DataContract]
    public class WPos
    {
        /// <summary>
        /// Identifier of the position.
        /// </summary>
        [DataMember]
        public long posId { get; set; }

        /// <summary>
        /// title.
        /// </summary>
        [DataMember]
        public String title { get; set; }

        /// <summary>
        /// industry.
        /// </summary>
        [DataMember]
        public String industry { get; set; }

        /// <summary>
        /// name.
        /// </summary>
        [DataMember]
        public String name { get; set; }

    }
}