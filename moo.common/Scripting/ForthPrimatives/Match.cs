using System.Threading.Tasks;
using moo.common.Database;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Match
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            MATCH ( s -- d ) 

            Takes string s, first checks all objects in the user's inventory, then checks all objects in the current room,
            as well as all exits that the player may use, and returns object d which contains string s.
            
            If nothing is found, d = #-1. If ambiguous, d = #-2. If HOME, d = #-3.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "MATCH requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.String || n1.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "MATCH requires the top parameter on the stack to be a string");

            var s = (string)n1.Value;

            var initMatch = Matcher.InitObjectSearch(parameters.Connection.GetPlayer(), s, Dbref.DbrefObjectType.Unknown, parameters.CancellationToken);
            Task<MatchResult> matcher;

            if (s[0] == '$')
            {
                matcher = initMatch.MatchRegistered();
            }
            else
            {
                matcher = initMatch
                    .MatchAllExits()
                    .MatchNeighbor()
                    .MatchPossession()
                    .MatchMe()
                    .MatchHere()
                    .MatchHome()
                    .MatchNil();
            }

            if (parameters.Process?.EffectiveMuckerLevel >= 4)
            {
                matcher = matcher
                    .MatchAbsolute();
            }

            // TODO: Wiz program permissions https://github.com/fuzzball-muck/fuzzball/blob/b0ea12f4d40a724a16ef105f599cb8b6a037a77a/src/p_db.c#L866

            Dbref result = await matcher.Result();
            if (result == Dbref.NOT_FOUND)
            {
                var a = 3;
                a++;
            }

            parameters.Stack.Push(new ForthDatum(result, 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}