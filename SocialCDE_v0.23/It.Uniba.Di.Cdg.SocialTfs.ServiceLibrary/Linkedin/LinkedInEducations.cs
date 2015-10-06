using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Linkedin
{
    public class LinkedInEducations
    {
        #region Attributes

        public LinkedInEducationsList educations { get; set; }

        #endregion

        public String[] GetEducationsName()
        {
            string[] result;

            if (educations == null)
                result = new string[0];
            else
            {
                string[] educationsName = new string[educations.values.Length];
                for (int i = 0; i < educations.values.Length; i++)
                {
                    educationsName[i] = educations.values[i].schoolName;
                }

                result = educationsName;
            }

            return result;
        }

        public class LinkedInEducationsList
        {
            public int _total { get; set; }
            public LinkedInEducationInfo[] values { get; set; }
        }

        public class LinkedInEducationInfo : IEdu
        {

            public long id { get; set; }
            public string schoolName { get; set; }
            public string fieldOfStudy { get; set; }

            long IEdu.eduId
            {
                get { return id; }
            }
        }

        
    }
}
