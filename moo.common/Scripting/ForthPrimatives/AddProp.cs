using System.Threading.Tasks;
using static ForthDatum;

public static class AddProp
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        ADDPROP ( d s1 s2 i -- ) 

        Sets property associated with s1 in object d. Note that if s2 is null "", then i will be used.
        Otherwise, s2 is always used. All four parameters must be on the stack; none may be omitted.

        If the effective user of the program does not control the object in question and the property
        begins with an underscore `_', the property cannot be changed. The same goes for properties
        beginning with a dot `.' which cannot be read without permission.
        */
        if (parameters.Stack.Count < 4)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ADDPROP requires four parameters");

        var i = parameters.Stack.Pop();
        if (i.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ADDPROP requires the top parameter on the stack to be an integer");

        var s2 = parameters.Stack.Pop();
        if (s2.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ADDPROP requires the second-to-top parameter on the stack to be a string");

        var s1 = parameters.Stack.Pop();
        if (s1.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ADDPROP requires the third-to-top parameter on the stack to be a string");

        var d = parameters.Stack.Pop();
        if (d.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ADDPROP requires the fourth-to-top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(d.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess || targetResult.value == null)
            return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {d.UnwrapDbref()}");

        var path = ((string?)s1.Value) ?? string.Empty;

        //if (!targetResult.value.IsControlledBy(parameters.Connection.Dbref) && path.Contains('_'))
        //    return new ForthPrimativeResult(ForthErrorResult.INSUFFICIENT_PERMISSION, $"Permission not granted to write protected property {path} on {d.UnwrapDbref()}");

        if (s2.isTrue())
            targetResult.value.SetPropertyPathValue(path, new ForthVariable((string?)s2.Value ?? string.Empty));
        else
            targetResult.value.SetPropertyPathValue(path, new ForthVariable(i.UnwrapInt()));

        return ForthPrimativeResult.SUCCESS;
    }
}