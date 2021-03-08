namespace moo.common
{
    public static class StringUtility
    {
        public static int? StringMatch(string src, string sub)
        {
            if (!string.IsNullOrEmpty(sub))
            {
                for (int i = 0; i < src.Length; i++)
                {
                    if (src.IndexOf(sub, i, System.StringComparison.OrdinalIgnoreCase) > -1)
                        return i;

                    /* else scan to beginning of next word */
                    while (i < src.Length && char.IsLetterOrDigit(src[i]))
                        i++;

                    while (i < src.Length && !char.IsLetterOrDigit(src[i]))
                        i++;
                }
            }
            return null;
        }
    }
}