using System;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{

    /// <summary>
    /// Rapresent a post in the microblog.
    /// </summary>
    public interface IPost
    {
        /// <summary>
        /// Identifier number of the post.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// The text of the post.
        /// </summary>
        String Text { get; }

        /// <summary>
        /// The creation date of the post.
        /// </summary>
        DateTime CreatedAt { get; }
    }
}
