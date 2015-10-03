using System;
using System.Diagnostics.Contracts;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary.StatusNet
{
    /// <summary>
    /// Rapresent a post in StatusNet.
    /// </summary>
    /// <remarks>
    /// All public attributes are matched by the JSON web response.
    /// In this way it's possible to serizalize automatically from the web response.
    /// </remarks>
    class StatusNetPost : IPost
    {
        #region Attributes

        public long id { get; set; }
        public String text { get; set; }
        public String created_at { get; set; }

        #endregion

        long IPost.Id
        {
            get { return id; }
        }

        String IPost.Text
        {
            get { return text; }
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

            char[] delimiterChars = { ' ', ':' };
            String[] words = d.Split(delimiterChars);
            int month = 0;
            switch (words[1])
            {
                case "Jan":
                    month = 1;
                    break;
                case "Feb":
                    month = 2;
                    break;
                case "Mar":
                    month = 3;
                    break;
                case "Apr":
                    month = 4;
                    break;
                case "May":
                    month = 5;
                    break;
                case "Jun":
                    month = 6;
                    break;
                case "Jul":
                    month = 7;
                    break;
                case "Aug":
                    month = 8;
                    break;
                case "Sep":
                    month = 9;
                    break;
                case "Oct":
                    month = 10;
                    break;
                case "Nov":
                    month = 11;
                    break;
                case "Dec":
                    month = 12;
                    break;
            }
            return new DateTime(Int32.Parse(words[7]), month, Int32.Parse(words[2]), Int32.Parse(words[3]), Int32.Parse(words[4]), Int32.Parse(words[5]));
        }
    }
}
