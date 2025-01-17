﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainUtils
{
    public class ByteHelper
    {
        public static byte[] GetBytesFromString(string str)
        {
            return Encoding.UTF8.GetBytes(str);

            //byte[] bytes = new byte[str.Length * sizeof(char)];
            //System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            //return bytes;
        }

        public static string GetStringFromBytes(byte[] bytes)
        {
            //char[] chars = new char[bytes.Length / sizeof(char)];
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            //return new string(chars);

            return Encoding.UTF8.GetString(bytes);
        }

        public static string GetStringFromBytesASCI(byte[] bytes)
        {
            //char[] chars = new char[bytes.Length / sizeof(char)];
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            //return new string(chars);

            return Encoding.ASCII.GetString(bytes);
        }

        public static byte[] GetBytesFromStringASCI(string ascistr)
        {
            return Encoding.ASCII.GetBytes(ascistr);
        }
    }
}