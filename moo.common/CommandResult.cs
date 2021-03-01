using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public struct CommandResult
{

    private static Regex parts = new Regex(@"^\s*(?<verb>[^\s]+)(?:(?: +(?<io>(?:.*?)) +(?<prepI>to) +(?<doI>.*)$)|(?: +(?:(?<prepD>above|beside|around|at|in|on|off|under) *)?(?<doD>.*)$))?", RegexOptions.Compiled);

    public String raw;
    private System.Text.RegularExpressions.Match match;

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
        return (text.Substring(0, pos) + replace + text.Substring(pos + search.Length)).TrimStart();
    }

    public bool hasIndirectObject() => !string.IsNullOrWhiteSpace(match.Groups["io"].Value);

    public bool hasDirectObject() => !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) || !string.IsNullOrWhiteSpace(match.Groups["doD"].Value);

    public string getDirectObject() => !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) ? match.Groups["doI"].Value : match.Groups["doD"].Value;

    public async Task<Dbref> ResolveDirectObject(PlayerConnection connection, CancellationToken cancellationToken)
    {
        var dobj = getDirectObject();

        if (String.Compare("me", dobj, true) == 0)
            return connection.Dbref;
        if (String.Compare("here", dobj, true) == 0)
            return connection.Location;
        if (Dbref.TryParse(dobj, out Dbref dbref))
            return dbref;

        var result = await connection.FindThingForThisPlayerAsync(dobj, cancellationToken);
        return result;
    }

    public override string ToString() => this.raw;
}