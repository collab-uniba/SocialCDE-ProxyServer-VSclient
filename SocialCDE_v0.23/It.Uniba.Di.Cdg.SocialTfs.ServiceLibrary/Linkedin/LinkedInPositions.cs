using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Linkedin
{
    class LinkedInPositions
    {
        #region Attributes

        public LinkedInPositionsList positions { get; set; }

        #endregion

        public string[] GetPositionsTitle()
        {
            string[] result;

            if (positions == null)
                result = new string[0];
            else
            {
                string[] positionsName = new string[positions.values.Length];
                for (int i = 0; i < positions.values.Length; i++)
                {
                    positionsName[i] = positions.values[i].title;
                }

                result = positionsName;
            }

            return result;
        }

        public class LinkedInPositionsList
        {
            public int _total { get; set; }
            public LinkedInEducationInfo[] values { get; set; }
        }

        public class LinkedInEducationInfo : IPos
        {
            public long id { get; set; }
            public string title { get; set; }
            public LinkedInPositionsIndustry company { get; set; }

            long IPos.posId
            {
                get { return id; }
            }

            string IPos.title
            {
                get { return title; }
            }

            string IPos.name
            {
                get { return company.name; }
            }

            string IPos.industry
            {
                get { return company.industry; }
            }
        }

        public class LinkedInPositionsIndustry
        {
            public string name { get; set; }
            public string industry { get; set; }
        }
        
    }
}
