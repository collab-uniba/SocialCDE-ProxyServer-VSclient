using System;
using System.Collections.Generic;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Linkedin
{
    public class LinkedInSkills
    {
        #region Attributes

        public LinkedInSkillsList skills { get; set; }

        #endregion

        public string[] GetSkillsName()
        {
            string[] result;

            if (skills == null)
                result = new string[0];
            else
            {
                string[] skillsName = new string[skills.values.Length];
                for (int i = 0; i < skills.values.Length; i++)
                {
                    skillsName[i] = skills.values[i].skill.name;
                }

                result = skillsName;
            }

            return result;
        }

        public class LinkedInSkillsList
        {
            public int _total { get; set; }
            public LinkedInSkillInfo[] values { get; set; }
        }

        public class LinkedInSkillInfo
        {
            public int id { get; set; }
            public LinkedInSkillName skill { get; set; }
        }

        public class LinkedInSkillName
        {
            public string name { get; set; }
        }
    }
}
