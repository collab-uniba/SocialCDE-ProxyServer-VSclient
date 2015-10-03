using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of edu data via REST requests.
    /// </summary>
    [DataContract]
    public class WEdu
    {
        /// <summary>
        /// Identifier of the edu.
        /// </summary>
        [DataMember]
        public long eduId { get; set; }

        /// <summary>
        /// FieldOfStudy.
        /// </summary>
        [DataMember]
        public String fieldOfStudy { get; set; }

        /// <summary>
        /// schoolName.
        /// </summary>
        [DataMember]
        public String schoolName { get; set; }
    }
}