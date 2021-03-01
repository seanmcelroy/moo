using System.Threading.Tasks;
using static ForthDatum;

public static class Match
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        MATCH ( s -- d ) 

        Takes string s, first checks all objects in the user's inventory, then checks all objects in the current room, as well as all exits that the player may use, and returns object d which contains string s. If nothing is found, d = #-1. If ambiguous, d = #-2. If HOME, d = #-3.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "MATCH requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "MATCH requires the top parameter on the stack to be a string");

        var s = (string?)n1.Value;

        if (s != null && string.Compare("me", s, true) == 0)
        {
            parameters.Stack.Push(new ForthDatum(parameters.Connection?.Dbref ?? Dbref.NOT_FOUND, 0));
            return ForthPrimativeResult.SUCCESS;
        }

        var inventoryMatch = await parameters.Connection.MatchAsync(s, parameters.CancellationToken);
        var locationResult = await ThingRepository.GetAsync<Container>(parameters.Connection.Location, parameters.CancellationToken);

        // TODO: Should only match the exits in a room I can actually use.

        var locationMatch = default(Dbref);
        if (locationResult.isSuccess && locationResult.value != null)
        {
            locationMatch = await locationResult.value.MatchAsync(s, parameters.CancellationToken);
        }

        parameters.Stack.Push(new ForthDatum(inventoryMatch | locationMatch, 0));
        return ForthPrimativeResult.SUCCESS;
    }
}