using System;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{

    /// <summary>
    /// Rapresent StackOverflow info about reputation.
    /// </summary>
    public interface IStack
    {
        /// <summary>
        /// Identifier number of the stackInfo.
        /// </summary>
        long user_id { get; }

        /// <summary>
        /// The reputationValue of the stackInfo.
        /// </summary>
        long reputationValue { get; }

        /// <summary>
        /// The bronze-badge-score of the stackInfo.
        /// </summary>
        long bronze { get; }

        /// <summary>
        /// The silver-badge-score of the stackInfo.
        /// </summary>
        long silver { get; }

        /// <summary>
        /// The gold-badge-score of the stackInfo.
        /// </summary>
        long gold { get; }

        /// <summary>
        /// The questionCount of the stackInfo.
        /// </summary>
        long questionCount { get; }

        /// <summary>
        /// The answerCount of the stackInfo.
        /// </summary>
        long answerCount { get; }
    }
}
