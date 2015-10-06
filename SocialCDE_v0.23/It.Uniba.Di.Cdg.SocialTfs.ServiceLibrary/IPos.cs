using System;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{

    /// <summary>
    /// Rapresent an education in the microblog.
    /// </summary>
    public interface IPos
    {
        /// <summary>
        /// Identifier number of the position.
        /// </summary>
        long posId { get; }

        /// <summary>
        /// The title.
        /// </summary>
        String title { get; }

        /// <summary>
        /// The industry.
        /// </summary>
        String industry { get; }

        /// <summary>
        /// The name.
        /// </summary>
        String name { get; }
    }
}