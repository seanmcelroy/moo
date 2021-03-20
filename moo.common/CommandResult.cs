using System;
using System.Text.RegularExpressions;

namespace moo.common
{
    public struct CommandResult
    {
        private static readonly Regex parts = new(@"^\s*(?<verb>[^\s]+)(?:(?: +(?<io>(?:.*?)) +(?<prepI>to) +(?<doI>.*)$)|(?: +(?:(?<prepD>above|beside|around|at|in|on|off|under) *)?(?<doD>.*)$))?", RegexOptions.Compiled);

        public string Raw { get; init; }
        private readonly Match match;

        public CommandResult(string raw)
        {
            Raw = raw;
            match = parts.Match(raw);
        }

        public string GetVerb() => match.Groups["verb"].Value;

        public string GetNonVerbPhrase()
        {
            var text = Raw;
            var search = GetVerb();
            var replace = "";

            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return (text.Substring(0, pos) + replace + text[(pos + search.Length)..]).TrimStart();
        }

        public bool HasIndirectObject() => !string.IsNullOrWhiteSpace(match.Groups["io"].Value);

        public bool HasDirectObject() => !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) || !string.IsNullOrWhiteSpace(match.Groups["doD"].Value);

        public string GetDirectObject() => !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) ? match.Groups["doI"].Value : match.Groups["doD"].Value;

        public override string ToString() => Raw;

        public override bool Equals(object? obj) => obj is CommandResult result &&
                   string.Compare(Raw, result.Raw, StringComparison.Ordinal) == 0;

        public override int GetHashCode() => string.GetHashCode(Raw);

        public static bool operator ==(CommandResult left, CommandResult right) => left.Equals(right);

        public static bool operator !=(CommandResult left, CommandResult right) => !(left == right);
    }
}