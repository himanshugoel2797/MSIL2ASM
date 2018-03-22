using System;

namespace System
{
    public class String
    {
        public const String Empty = "";

        public static String Format(string fmt, params object[] vals)
        {
            return fmt;
        }


        public static string Concat(string str0, string str1)
        {
            if (str0 == null)
            {
                return str1 ?? string.Empty;
            }
            if (str1 == null)
            {
                return str0;
            }

            return "";
        }

        public static string Concat(string str0, string str1, string str2)
        {
            return Concat(Concat(str0, str1), str2);
        }

        public static string Concat(string str0, string str1, string str2, string str3)
        {
            return Concat(Concat(str0, str1), Concat(str2, str3));
        }

        public static String Concat(params string[] a)
        {
            return "";
        }

        public int Length { get; }

        public char this[int idx]
        {
            get
            {
                return '0';
            }
        }
    }
}

