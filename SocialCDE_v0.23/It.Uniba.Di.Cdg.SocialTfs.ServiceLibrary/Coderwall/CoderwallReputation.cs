using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Coderwall
{
    public class CoderwallReputation : IReputation
    {

        #region Attributes

        public int endorsements { get; set; }

        #endregion


         public int coderwallEndorsements
        {
            get { return this.endorsements; }
        }

        public int reputationId { get; set; }

        public int stackReputationValue { get; set; }

        public int stackReputationLevel { get; set; }

        public int stackAnswer { get; set; }

        public int stackQuestion { get; set; }

        public int stackBronze { get; set; }

        public int stackSilver { get; set; }

        public int stackGold { get; set; }

        public int ohlohKudoScore { get; set; }

        public int ohlohKudoRank { get; set; }

        public bool ohlohBigcheese { get; set; }

        public bool ohlohOrgman { get; set; }

        public int ohlohFosser { get; set; }

        public int ohlohStacker { get; set; }

        public int linkedinRecommenders { get; set; }

        public int linkedinRecommendations { get; set; }
    }
}
