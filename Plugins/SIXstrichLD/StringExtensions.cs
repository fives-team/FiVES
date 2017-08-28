namespace SIXstrichLDPlugin
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
