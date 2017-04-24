using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIXstrichLDPlugin
{
    static class StringExtensions
    {
        public static string RemoveFromTail(this string s, string tailString)
        {
            if (s.EndsWith(tailString))
            {
                return s.Substring(0, s.Length - tailString.Length);
            }
            else
            {
                return s;
            }
        }
    }
}
