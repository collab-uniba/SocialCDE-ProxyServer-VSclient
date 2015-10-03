using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of edu data via REST requests.
    /// </summary>
    [DataContract]
    public class WReputation
    {
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public long reputationId { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? stackReputationValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? stackAnswer { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? stackQuestion { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? stackBronze { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? stackSilver { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? stackGold { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? coderwallEndorsements { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? ohlohKudoRank { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? ohlohKudoScore { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public bool? ohlohBigcheese { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public bool? ohlohOrgman { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? ohlohFosser { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? ohlohStacker { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? linkedinRecommenders { get; set; }
        /// <summary>
        ///
        /// </summary>
        [DataMember]
        public int? linkedinRecommendations { get; set; }

    }
}