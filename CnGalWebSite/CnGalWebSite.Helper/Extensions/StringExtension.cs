using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.Extensions
{
    public static class StringExtension
    {
        public static string Abbreviate(this string str, int length)
        {
            if (str == null)
            {
                return "";
            }
            if (str.Length < length * 2.5)
            {
                return str;
            }
            else
            {
                return string.Concat(str.AsSpan(0, (int)(length * 2.5)), "...");
            }
        }
    }
}
