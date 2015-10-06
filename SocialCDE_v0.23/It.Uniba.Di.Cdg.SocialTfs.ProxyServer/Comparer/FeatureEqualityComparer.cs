using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace It.Uniba.Di.Cdg.SocialTfs.ProxyServer.Comparer
{
    public class FeatureEqualityComparer : IEqualityComparer<Feature>
    {
        public bool Equals(Feature x, Feature y)
        {
            return x.pk_name == y.pk_name;
        }

        public int GetHashCode(Feature obj)
        {
            return this.GetHashCode();
        }
    }
}