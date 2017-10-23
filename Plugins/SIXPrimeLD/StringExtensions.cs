// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
namespace SIXPrimeLDPlugin
{
    static class StringExtensions
    {
        /// <summary>
        /// Remove a substring at the end of a string
        /// </summary>
        /// <param name="s">string to remove from</param>
        /// <param name="tailString">substring which shall be removed</param>
        /// <returns>trimmed string or unchanged string if the substring does not exist at the end of the string</returns>
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
