using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace It.Uniba.Di.Cdg.SocialTfs.ServiceLibrary
{
    public class OAuthAccessData
    {
        /// <summary>
        /// URI associated to the web request.
        /// </summary>
        public string RequestUri { get; internal set; }

        /// <summary>
        /// The access token.
        /// </summary>
        public string AccessToken { get; internal set; }

        /// <summary>
        /// The access secret.
        /// </summary>
        public string AccessSecret { get; internal set; }
    }
}
