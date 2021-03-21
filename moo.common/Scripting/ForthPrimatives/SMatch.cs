using System.Text.RegularExpressions;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class SMatch
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            Takes a string s, and a string pattern, s2, to check against. Returns true if the string fits the pattern. This is case insensitive. In the pattern string, the following special characters will do as follows:

            * A '?' matches any single character.

            * A '*' matches any number of any characters.

            * '{word1|word2|etc}' will match a single word, if it is one of those
            given, separated by | characters, between the {}s. A word ends with
            a space or at the end of the string. The given example would match
            either the words "word1", "word2", or "etc".

            {} word patterns will only match complete words: "{foo}*" and "{foo}p"
            do not match "foop" and "*{foo}" and "p{foo}" do not match "pfoo".

            {} word patterns can be easily meaningless; they will match nothing
            if they:
            (a) contains spaces,
            (b) do not follow a wildcard, space or beginning of string,
            (c) are not followed by a wildcard, space or end of string.

            * If the first char of a {} word set is a '^', then it will match a single
            word if it is NOT one of those contained within the {}s. Example:

            '{^Foxen|Fiera}' will match any single word EXCEPT for Foxen or Fiera.

            * '[aeiou]' will match a single character as long as it is one of those
            contained between the []s. In this case, it matches any vowel.

            * If the first char of a [] char set is a '^', then it will match a single
            character if it is NOT one of those contained within the []s. Example:

            '[^aeiou]' will match any single character EXCEPT for a vowel.
            * If a [] char set contains two characters separated by a '-', then it will

            match any single character that is between those two given characters.
            Example: '[a-z0-9_]' would match any single character between 'a' and
            'z', inclusive, any character between '0' and '9', inclusive, or a '_'.

            * The '\' character will disable the special meaning of the character that
            follows it, matching it literally.

            Example patterns:
            "d*g" matches "dg", "dog", "doog", "dorfg", etc.
            "d?g" matches "dog", "dig" and "dug" but not "dg" or "drug".
            "M[rs]." matches "Mr." and "Ms."
            "M[a-z]" matches "Ma", "Mb", etc.
            "[^a-z]" matches anything but an alphabetical character.
            "{Moira|Chupchup}*" matches "Moira snores" and "Chupchup arghs."
            "{Moira|Chupchup}*" does NOT match "Moira' snores".
            "{Foxen|Lynx|Fier[ao]} *t[iy]ckle*\?" Will match any string starting
            with 'Foxen', 'Lynx', 'Fiera', or 'Fiero', that contains either 'tickle'
            or 'tyckle' and ends with a '?'. 
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SMATCH requires three parameters");

            var n2 = parameters.Stack.Pop();
            if (n2.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SMATCH requires the second-to-top parameter on the stack to be a string");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SMATCH requires the third-to-top parameter on the stack to be a string");

            var input = (string?)n1.Value ?? string.Empty;
            var pattern = (string?)n2.Value ?? string.Empty;

            // Non-negation version
            var regex = Regex.Replace(pattern, @"(?<=^|\?|\*|\s)(\{(?:(?:(?:[^|\^|\s][^|\s]*\|){1,20}[^}\s]+)|[^|\^|\s}]+[^|\s}]+)\})(?=\?|\*|\s|$)", "=~~~~~$1~~~~~=", RegexOptions.Compiled);

            // Negation version
            regex = Regex.Replace(regex, @"(?<=^|\?|\*|\s)(\{\^(?:(?:(?:[^|\^|\s][^|\s]*\|){1,20}[^}\s]+)|[^|\^|\s}]+[^|\s}]+)\})(?=\?|\*|\s|$)", "!~~~~~$1~~~~~!", RegexOptions.Compiled);

            regex = regex.Replace(".", "\\.");

            regex = Regex.Replace(regex, @"(?<!\\)\?", ".") // ? to . if not escaped as \?
                .Replace("=~~~~~{", "(")
                .Replace("}~~~~~=", @")(?=\s|$)")
                .Replace("!~~~~~{^", @"(?<=^|\s)(?!(")
                .Replace("}~~~~~!", @"))[^\s\b]*(?=\s|$)")
                .Replace("*", ".*");

            var result = Regex.IsMatch(input, regex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            parameters.Stack.Push(new ForthDatum(result ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}