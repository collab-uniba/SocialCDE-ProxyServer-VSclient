using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of stackInfo data via REST requests.
    /// </summary>
    [DataContract]
    public class WStack
    {
        /// <summary>
        /// Identifier of the stackInfo.
        /// </summary>
        [DataMember]
        public long user_id { get; set; }

        /// <summary>
        /// reputationValue.
        /// </summary>
        [DataMember]
        public long reputationValue { get; set; }

        /// <summary>
        /// bronze.
        /// </summary>
        [DataMember]
        public long bronze { get; set; }

        /// <summary>
        /// silver.
        /// </summary>
        [DataMember]
        public long silver { get; set; }

        /// <summary>
        /// gold.
        /// </summary>
        [DataMember]
        public long gold { get; set; }

        /// <summary>
        /// questionCount.
        /// </summary>
        [DataMember]
        public long questionCount { get; set; }

        /// <summary>
        /// answerCount.
        /// </summary>
        [DataMember]
        public long answerCount { get; set; }
    }
}