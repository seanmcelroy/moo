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

        public static (string? inner, int endOfClosingTag) FindInnerXml(this string whole, string tagName)
        {
            var innerStart = whole.IndexOf($"<{tagName}>");
            if (innerStart < 0)
                return (null, -1);

            var innerEnd = whole[innerStart..].FindIndexOfClosingTag(tagName);
            if (innerEnd == -1)
                return (null, -1);

            return (whole[(innerStart + 2 + tagName.Length)..(innerStart + innerEnd)], innerStart + innerEnd + 2 + tagName.Length + 1);
        }

        public static int FindIndexOfClosingTag(this string whole, string tagName)
        {
            var openingTag = whole.IndexOf($"<{tagName}>");
            if (openingTag < 0)
                return -1;

            var idx = openingTag;
            var depth = 0;
            do
            {
                var nextOpeningTag = whole.IndexOf($"<{tagName}>", idx + 1);
                var nextClosingTag = whole.IndexOf($"</{tagName}>", idx + 1);
                if (nextClosingTag == -1)
                    return -1;
                if (nextClosingTag < nextOpeningTag || nextOpeningTag == -1)
                {
                    idx = nextClosingTag;
                    if (depth == 0)
                        return idx;
                    depth--;
                }
                else
                {
                    idx = nextOpeningTag;
                    depth++;
                }

            } while (idx > -1 && idx <= whole.Length);
            return -1;
        }
    }
}