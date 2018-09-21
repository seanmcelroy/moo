using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthPrimativeResult;
using static Property;

public static class GetPropStr
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        GETPROPSTR ( d s -- s ) 

        s must be a string. Retrieves string associated with property s in object d. If the property is cleared, "" (null string) is returned.
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "GETPROPVAL requires two parameters");

        var sPath = parameters.Stack.Pop();
        if (sPath.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the top parameter on the stack to be a string");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the second-to-top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess)
            return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

        var property = await targetResult.value.GetPropertyPathValueAsync((string)sPath.Value, parameters.CancellationToken);
        if (property.Equals(default(Property)) || property.Type != PropertyType.String)
        {
            parameters.Stack.Push(new ForthDatum(""));
            return ForthPrimativeResult.SUCCESS;
        }

        parameters.Stack.Push(new ForthDatum((string)property.Value));
        return ForthPrimativeResult.SUCCESS;
    }
}