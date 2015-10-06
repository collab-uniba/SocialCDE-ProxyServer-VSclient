using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// This class rapresents a generic work item menaged by a Team Foundation Server system.
    /// </summary>
    public class SWorkItem
    {        
        /// <summary>
        /// The name of the work item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All the users involved in the work item.
        /// </summary>
        public string[] InvolvedUsers { get; set; }
    }
}
