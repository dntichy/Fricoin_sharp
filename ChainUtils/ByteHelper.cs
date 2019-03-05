using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainUtils
{
    public class ByteHelper
    {
        public static string GetStringFromBytes(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("x"));
            }

            return result.ToString();
        }

        public static byte[] GetBytesFromString(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}