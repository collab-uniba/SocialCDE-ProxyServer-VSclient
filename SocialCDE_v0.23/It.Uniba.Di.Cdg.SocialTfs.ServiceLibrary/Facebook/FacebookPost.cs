using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Facebook
{
    class FacebookPost : IPost
    {
        #region Attributes

        public long id { get; set; }
        public String message { get; set; }
        public String updated_time { get; set; }

        #endregion

        long IPost.Id
        {
            get { return id; }
        }

        string IPost.Text
        {
            get { return message; }
        }

        DateTime IPost.CreatedAt
        {
            get { return ParseDate(updated_time); }
        }

        /// <summary>
        /// Convert the date returned by the web in the standard <see cref="DateTime"/> format.
        /// </summary>
        /// <param name="d">Date as String.</param>
        /// <returns>Date in a standard format.</returns>
        private DateTime ParseDate(String d)
        {
            Contract.Requires(!String.IsNullOrEmpty(d));
            char[] delimiterChars = { '-', 'T', ':', '+' };
            String[] words = d.Split(delimiterChars);
            return new DateTime(Int32.Parse(words[0]), Int32.Parse(words[1]), Int32.Parse(words[2]), Int32.Parse(words[3]), Int32.Parse(words[4]), Int32.Parse(words[5]));
        }
    }

    class FacebookTimeline
    {
        public FacebookPost[] data { get; set; }
    }
}
