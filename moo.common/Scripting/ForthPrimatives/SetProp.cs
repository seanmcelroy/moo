using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthPrimativeResult;
using static Property;

public static class SetProp
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        SETPROP (d s ? -- ) 

        Stores a lock, dbref, integer, or string into the named property on the given object. Permissions are the same as for ADDPROP.
        */
        if (parameters.Stack.Count < 3)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SETPROP requires four parameters");

        var v = parameters.Stack.Pop();

        var s = parameters.Stack.Pop();
        if (s.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SETPROP requires the second-to-top parameter on the stack to be a string");

        var d = parameters.Stack.Pop();
        if (d.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SETPROP requires the third-to-top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(d.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess)
            return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {d.UnwrapDbref()}");

        var path = ((string)s.Value);

        targetResult.value.SetPropertyPathValue(path, new ForthVariable(v));

        return ForthPrimativeResult.SUCCESS;
    }
}