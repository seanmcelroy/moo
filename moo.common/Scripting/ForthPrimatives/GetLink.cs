using System.Threading.Tasks;
using static ForthDatum;

public static class GetLink
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        GETLINK ( d -- d' ) 

        Returns what object d is linked to, or #-1 if d is unlinked. The interpretation of link depends on the
        type of d: for an exit, returns the room, player, action, or thing that the exit is linked to.
        
        For a player or thing, it returns its `home', and for rooms returns the drop-to.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "GETLINK requires one parameter");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETLINK requires the top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess || targetResult.value == null)
            return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

        parameters.Stack.Push(new ForthDatum(targetResult.value.Links[0], 0));
        return ForthPrimativeResult.SUCCESS;
    }
}