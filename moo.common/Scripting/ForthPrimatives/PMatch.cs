using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class PMatch
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        PMATCH (s -- d) 

        Takes a name and returns the dbref of the player. If the name does not match that of a player, #-1 is returned.
        */

        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PMATCH requires one parameter");

        var s = parameters.Stack.Pop();
        if (s.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PMATCH requires the top parameter on the stack to be a string");

        var name = (string)s.Value;

        var matches = parameters.Server.GetConnectionPlayers()
            .Where(x => string.Compare(x.Item2, name, StringComparison.InvariantCultureIgnoreCase) == 0)
            .Distinct()
            .ToArray();

        if (matches.Length == 0)
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND));
        else
            parameters.Stack.Push(new ForthDatum(matches[0].Item1));
        return ForthPrimativeResult.SUCCESS;
    }
}