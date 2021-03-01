using System.Threading.Tasks;
using static ForthDatum;

public static class UnparseObj
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        UNPARSEOBJ ( d -- s )

        Returns the name-and-flag string for an object. It always has the dbref and flag string after the name, even if the player doesn't control the object. For example: "One(#1PW)"
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "UNPARSEOBJ requires one parameter");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "UNPARSEOBJ requires the top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess || targetResult.value == null)
        {
            parameters.Stack.Push(new ForthDatum(string.Empty));
            return ForthPrimativeResult.SUCCESS;
        }

        parameters.Stack.Push(new ForthDatum(targetResult.value.UnparseObject(), sTarget.FileLineNumber, null, sTarget.WordName, sTarget.WordLineNumber));
        return ForthPrimativeResult.SUCCESS;
    }
}