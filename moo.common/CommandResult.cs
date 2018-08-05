using System;
using System.Text.RegularExpressions;
using static Dbref;

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

    public string getVerb()
    {
        return match.Groups["verb"].Value;
    }

    public string getNonVerbPhrase()
    {
        var result = this.raw.Trim().Replace(getVerb(), "").Trim();
        return result.Length == 0 ? null : result;
    }

    public bool hasIndirectObject()
    {
        return !string.IsNullOrWhiteSpace(match.Groups["io"].Value);
    }

    public bool hasDirectObject()
    {
        return !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) || !string.IsNullOrWhiteSpace(match.Groups["doD"].Value);
    }

    public string getDirectObject()
    {
        return !string.IsNullOrWhiteSpace(match.Groups["doI"].Value) ? match.Groups["doI"].Value : match.Groups["doD"].Value;
    }

    public Dbref resolveDirectObject(Dbref playerId, Dbref playerLocation)
    {
        var dobj = getDirectObject();

        if (String.Compare("me", dobj, true) == 0)
            return playerId;
        if (String.Compare("here", dobj, true) == 0)
            return playerLocation;
        if (Regex.IsMatch(dobj, @"#\d+[A-Z]?"))
        {
            DbrefObjectType type;
            switch (dobj[dobj.Length - 1])
            {
                case 'E':
                    type = DbrefObjectType.Exit;
                    break;
                case 'P':
                    type = DbrefObjectType.Player;
                    break;
                case 'R':
                    type = DbrefObjectType.Room;
                    break;
                default:
                    type = DbrefObjectType.Thing;
                    break;
            }

            return new Dbref(int.Parse(dobj.Substring(1)), type);
        }

        return Dbref.NOT_FOUND;
    }
}