using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace It.Uniba.Di.Cdg.SocialTfs.SharedLibrary
{
    public static class Crypt
    {
        public static byte[] Encrypt(string str)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] convert = System.Text.Encoding.Unicode.GetBytes(str);
            byte[] result = sha.ComputeHash(convert);
            return result;

        }
    }
}
