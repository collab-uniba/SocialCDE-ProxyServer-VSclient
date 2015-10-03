using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.Yammer
{
    class YammerPost : IPost
    {
        #region Attributes

        public long id { get; set; }
        public YammerMessage body { get; set; }
        public string created_at { get; set; }

        #endregion

        long IPost.Id
        {
            get { return id; }
        }

        string IPost.Text
        {
            get { return body.plain; }
        }

        DateTime IPost.CreatedAt
        {
            get { return ParseDate(created_at); }
        }

        /// <summary>
        /// Convert the date returned by the web in the standard <see cref="DateTime"/> format.
        /// </summary>
        /// <param name="d">Date as String.</param>
        /// <returns>Date in a standard format.</returns>
        private DateTime ParseDate(String d)
        {
            Contract.Requires(!String.IsNullOrEmpty(d));

            char[] delimiterChars = { '/', ':', ' ' };
            String[] words = d.Split(delimiterChars);
            return new DateTime(Int32.Parse(words[0]), Int32.Parse(words[1]), Int32.Parse(words[2]), Int32.Parse(words[3]), Int32.Parse(words[4]), Int32.Parse(words[5]));
        }
    }

    class YammerTimeline
    {
        public YammerPost[] messages { get; set; }
    }

    class YammerMessage
    {
        public string plain { get; set; }
    }
}
