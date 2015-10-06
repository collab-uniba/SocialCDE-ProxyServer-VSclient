using System;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{

    /// <summary>
    /// Rapresent an education in the microblog.
    /// </summary>
    public interface IReputation
    {
        /// <summary>
        /// Identifier number of the reputation.
        /// </summary>
        int reputationId { get; }

        /// <summary>
        /// 
        /// </summary>
        int stackReputationValue { get; }

        /// <summary>
        /// 
        /// </summary>
        int stackAnswer { get; }

        /// <summary>
        /// 
        /// </summary>
        int stackQuestion { get; }

        /// <summary>
        /// 
        /// </summary>
        int stackBronze { get; }

        /// <summary>
        /// 
        /// </summary>
        int stackSilver { get; }

        /// <summary>
        /// 
        /// </summary>
        int stackGold { get; }

        /// <summary>
        /// 
        /// </summary>
        int coderwallEndorsements { get; }

        /// <summary>
        /// 
        /// </summary>
        int ohlohKudoScore { get; }

        /// <summary>
        /// 
        /// </summary>
        int ohlohKudoRank { get; }

        /// <summary>
        /// 
        /// </summary>
        bool ohlohBigcheese { get; }

        /// <summary>
        /// 
        /// </summary>
        bool ohlohOrgman { get; }

        /// <summary>
        /// 
        /// </summary>
        int ohlohFosser { get; }

        /// <summary>
        /// 
        /// </summary>
        int ohlohStacker { get; }

        /// <summary>
        /// 
        /// </summary>
        int linkedinRecommenders { get; }

        /// <summary>
        /// 
        /// </summary>
        int linkedinRecommendations { get; }
    }
}