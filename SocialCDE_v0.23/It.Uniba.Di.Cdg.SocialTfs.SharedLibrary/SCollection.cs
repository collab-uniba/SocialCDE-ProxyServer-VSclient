using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// This class rapresents a collection in a Team Foundation Server system.
    /// </summary>
    public class SCollection
    {
        /// <summary>
        /// The URI of the collection.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// All the work items contained in the collection.
        /// </summary>
        public SWorkItem[] WorkItems { get; set; }

        /// <summary>
        /// All the files contained in the collection.
        /// </summary>
        public SFile[] Files { get; set; }
    }
}
