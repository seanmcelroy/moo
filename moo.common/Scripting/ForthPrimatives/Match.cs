using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public static class Match
{
    public static async Task<ForthProgramResult> ExecuteAsync(Stack<ForthDatum> stack, PlayerConnection connection, CancellationToken cancellationToken)
    {
        /*
        MATCH ( s -- d ) 

        Takes string s, first checks all objects in the user's inventory, then checks all objects in the current room, as well as all exits that the player may use, and returns object d which contains string s. If nothing is found, d = #-1. If ambiguous, d = #-2. If HOME, d = #-3.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "MATCH requires one parameter");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "MATCH requires the top parameter on the stack to be a string");

        var s = (string)n1.Value;

        if (string.Compare("me", s, true) == 0){
            stack.Push(new ForthDatum(connection.Dbref, 0));
            return default(ForthProgramResult);
        }

        var inventoryMatch = await connection.MatchAsync(s, cancellationToken);
        var locationResult = await ThingRepository.GetAsync<Container>(connection.Location, cancellationToken);

        // TODO: Should only match the exits in a room I can actually use.

        var locationMatch = default(Dbref);
        if (locationResult.isSuccess)
        {
            locationMatch = await locationResult.value.MatchAsync(s, cancellationToken);
        }

        stack.Push(new ForthDatum(inventoryMatch | locationMatch, 0));
        return default(ForthProgramResult);
    }
}