using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Ohloh
{
    public class OhlohReputation : IReputation
    {

        #region Attributes

        public int items_returned { get; set; } //aggiunto dopo
        public string id { get; set; }
        public string name { get; set; }
        public object about { get; set; }
        public string login { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string homepage_url { get; set; }
        public string twitter_account { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string avatar_url { get; set; }
        public string email_sha1 { get; set; }
        public string posts_count { get; set; }
        public string location { get; set; }
        public string country_code { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public KudoScore kudo_score { get; set; }
        public Languages languages { get; set; }
        public Badges badges { get; set; }
        
        #endregion

        public int ohlohKudoScore
        {
            get { return this.items_returned; }
        }

        public int ohlohKudoRank
        {
            get { return this.kudo_score.kudo_rank; }
        }

        public bool ohlohBigcheese { get; set; }

        public bool ohlohOrgman { get; set; }

        public int ohlohFosser  { get; set; }
         
        public int ohlohStacker { get; set; }



        public int coderwallEndorsements { get; set; }

        public int reputationId { get; set; }

        public int stackReputationValue { get; set; }

        public int stackReputationLevel { get; set; }

        public int stackAnswer { get; set; }

        public int stackQuestion { get; set; }

        public int stackBronze { get; set; }

        public int stackSilver { get; set; }

        public int stackGold { get; set; }

        public int linkedinRecommenders { get; set; }

        public int linkedinRecommendations { get; set; }


        public class Badge
        {
            public string name { get; set; }
            public string level { get; set; }
            public string description { get; set; }
            public string image_url { get; set; }
            public string pips_url { get; set; }
        }

        public class Badges
        {
            public List<Badge> badge { get; set; }
        }

        public class KudoScore
        {
            public int kudo_rank { get; set; }
            public string position { get; set; }
        }

        public class Language
        {
            public string @color { get; set; }
            public string name { get; set; }
            public string experience_months { get; set; }
            public string total_commits { get; set; }
            public string total_lines_changed { get; set; }
            public string comment_ratio { get; set; }
        }

        public class Languages
        {
            public List<Language> language { get; set; }
        }

        public string[] GetOhlohLanguagesName()
        {
            string[] result;

            if (languages == null)
                result = new string[0];
            else
            {
                string[] languagesName = new string[languages.language.Count];
                for (int i = 0; i < languages.language.Count; i++)
                {
                    languagesName[i] = languages.language[i].name;
                }

                result = languagesName;
            }

            return result;
        }

        public string[] GetOhlohBadges()
        {
            string[] result;

            if (badges == null)
                result = new string[0];
            else
            {
                string[] allBadges = new string[badges.badge.Count];
                for (int i = 0; i < badges.badge.Count; i++)
                {
                    allBadges[i] = badges.badge[i].name;
                }

                result = allBadges;
            }

            return result;
        }

    }
}
