using System;
using System.Text.RegularExpressions;

namespace moo.common
{
    public struct CommandResult
    {

        private static readonly Regex parts = new(@"^\s*(?<verb>[^\s]+)(?:(?: +(?<io>(?:.*?)) +(?<prepI>to) +(?<doI>.*)$)|(?: +(?:(?<prepD>above|beside|around|at|in|on|off|under) *)?(?<doD>.*)$))?", RegexOptions.Compiled);

        public String raw;
        private readonly System.Text.RegularExpressions.Match match;

        public CommandResult(String raw)
        {
            this.raw = raw;
            this.match = parts.Match(raw);
        }

        public string getVerb() => match.Groups["verb"].Value;

        public string getNonVerbPhrase()
        {
            var text = this.raw;
            var search = this.getVerb();
            var replace = "";

            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return (text.Substring(0, pos) + replace + text[(pos + search.Length)..]).TrimStart();
        }

        public bool hasIndirectObject() => !string.IsNullOrWhiteSpace(match.Groups["io"].Value);

        public bool hasDirectObject() => !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) || !string.IsNullOrWhiteSpace(match.Groups["doD"].Value);

        public string getDirectObject() => !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) ? match.Groups["doI"].Value : match.Groups["doD"].Value;

        public override string ToString() => this.raw;
    }
}