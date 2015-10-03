using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// This class rapresents a generic file menaged by a Team Foundation Server system.
    /// </summary>
    public class SFile
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All the users involved in the file.
        /// </summary>
        /// <remarks>
        /// An involved user is an user that have checked in at least one changeset.
        /// </remarks>
        public string[] InvolvedUsers { get; set; }
    }
}
