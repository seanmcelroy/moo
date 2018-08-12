using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class PartPMatch
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        PART_PMATCH (s -- d) 

        Takes a player name, or the first part of the name, and matches it against the names of the players who are currently online.
        If the given string is a prefix of the name of a player who is online, then their dbref is returned.
        If two players could be matched by the given string, it returns a #-2.
        If None of the players online match, then it returns a #-1.
        */

        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PART_PMATCH requires one parameter");

        var s = parameters.Stack.Pop();
        if (s.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PART_PMATCH requires the top parameter on the stack to be a string");

        var prefix = (string)s.Value;

        var matches = parameters.Server.GetConnectionPlayers()
            .Where(x => x.Item2.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            .Distinct()
            .ToArray();

        if (matches.Length == 0)
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND));
        else if (matches.Length > 1)
            parameters.Stack.Push(new ForthDatum(Dbref.AMBIGUOUS));
        else
            parameters.Stack.Push(new ForthDatum(matches[0].Item1));

        return ForthPrimativeResult.SUCCESS;
    }
}