using System.Threading.Tasks;
using static ForthDatum;

public static class Name
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        NAME ( d -- s ) 

        Takes object d and returns its name (@name) string field.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "NAME requires two parameters");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NAME requires the top parameter on the stack to be a dbref");


        var target = n1.UnwrapDbref();
        var targetResult = await ThingRepository.GetAsync<Thing>(target, parameters.CancellationToken);

        if (!targetResult.isSuccess || targetResult.value == null)
        {
            parameters.Stack.Push(new ForthDatum(""));
            return ForthPrimativeResult.SUCCESS;
        }

        parameters.Stack.Push(new ForthDatum(targetResult.value.name));
        return ForthPrimativeResult.SUCCESS;
    }
}