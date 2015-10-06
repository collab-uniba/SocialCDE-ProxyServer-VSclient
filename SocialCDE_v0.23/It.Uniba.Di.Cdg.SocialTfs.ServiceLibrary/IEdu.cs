using System;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{

    /// <summary>
    /// Rapresent an education in the microblog.
    /// </summary>
    public interface IEdu
    {
        /// <summary>
        /// Identifier number of the edu.
        /// </summary>
        long eduId { get; }

        /// <summary>
        /// The text of the edu.
        /// </summary>
        String fieldOfStudy { get; }

        /// <summary>
        /// The schoolname of the edu.
        /// </summary>
        String schoolName { get; }
    }
}
