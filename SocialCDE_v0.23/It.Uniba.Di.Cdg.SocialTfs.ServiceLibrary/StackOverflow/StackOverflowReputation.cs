using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.StackOverflow
{
    public class StackOverflowReputation : IReputation
    {
        #region Attributes

        public BadgeCounts badge_counts { get; set; }
        public int answer_count { get; set; }
        public int question_count { get; set; }
        public int reputation { get; set; }

        #endregion

        public int reputationId { get; set; }
        
        public int stackReputationValue
        {
            get { return this.reputation; }
        }

        public int stackAnswer
        {
            get { return this.answer_count; }
        }

        public int stackQuestion
        {
            get { return this.question_count; }
        }

        public int stackBronze
        {
            get { return this.badge_counts.bronze; }
        }

        public int stackSilver
        {
            get { return this.badge_counts.silver; }
        }

        public int stackGold
        {
            get { return this.badge_counts.gold; }
        }

        public int coderwallEndorsements { get; set; }
 
        public int ohlohKudoRank { get; set; }

        public int ohlohKudoScore { get; set; }

        public bool ohlohBigcheese { get; set; }

        public bool ohlohOrgman { get; set; }

        public int ohlohFosser { get; set; }

        public int ohlohStacker { get; set; }

        public int linkedinRecommenders { get; set; }

        public int linkedinRecommendations { get; set; }
        
    }

    public class BadgeCounts
    {
        public int bronze { get; set; }
        public int silver { get; set; }
        public int gold { get; set; }
    }
}
