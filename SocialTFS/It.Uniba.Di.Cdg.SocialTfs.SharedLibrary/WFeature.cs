using System.Runtime.Serialization;
using System;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of feature data via REST requests.
    /// </summary>
    [DataContract]
    public class WFeature
    {
        /// <summary>
        /// Name of the feature.
        /// </summary>
        [DataMember]
        public String Name { get; set; }

        /// <summary>
        /// Description of the Feature.
        /// </summary>
        [DataMember]
        public String Description { get; set; }

        /// <summary>
        /// True if the current user have chosed the feature, false otherwise.
        /// </summary>
        [DataMember]
        public bool IsChosen { get; set; }
    }
}