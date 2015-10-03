using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    /// <summary>
    /// A wrapper to allow the transmission of user visibility data via REST requests.
    /// </summary>
    [DataContract]
    public class WHidden
    {
        /// <summary>
        /// Is hidden in suggestions.
        /// </summary>
        [DataMember]
        public bool Suggestions { get; set; }

        /// <summary>
        /// Is hidden in dynamic timeline.
        /// </summary>
        [DataMember]
        public bool Dynamic { get; set; }

        /// <summary>
        /// Is hidden in interactive timeline.
        /// </summary>
        [DataMember]
        public bool Interactive { get; set; }
    }
}
