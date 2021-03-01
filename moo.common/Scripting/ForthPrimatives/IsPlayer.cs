using System.Threading.Tasks;
using static ForthDatum;

public static class IsPlayer
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        PLAYER? ( d -- i ) 

        Returns 1 if object d is a player object, otherwise returns 0. If the dbref is that of an invalid object, it will return 0.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PLAYER? requires one parameter");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PLAYER? requires the top parameter on the stack to be a dbref");

        if (sTarget.UnwrapDbref().ToInt32() < 0)
        {
            parameters.Stack.Push(new ForthDatum(0));
            return ForthPrimativeResult.SUCCESS;
        }

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess || targetResult.value == null)
        {
            parameters.Stack.Push(new ForthDatum(0));
            return ForthPrimativeResult.SUCCESS;
        }

        if (targetResult.value.type != (int)Dbref.DbrefObjectType.Player)
            parameters.Stack.Push(new ForthDatum(0));
        else
            parameters.Stack.Push(new ForthDatum(1));

        return ForthPrimativeResult.SUCCESS;
    }
}