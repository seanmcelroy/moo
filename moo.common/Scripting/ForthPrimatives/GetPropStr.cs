using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;
using static Property;

public static class GetPropStr
{
    public static async Task<ForthProgramResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        GETPROPSTR ( d s -- s ) 

        s must be a string. Retrieves string associated with property s in object d. If the property is cleared, "" (null string) is returned.
        */
        if (parameters.Stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "GETPROPVAL requires two parameters");

        var sPath = parameters.Stack.Pop();
        if (sPath.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the top parameter on the stack to be an string");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the second-to-top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
        if (!targetResult.isSuccess)
            return new ForthProgramResult(ForthProgramErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

        var property = targetResult.value.GetPropertyPathValue((string)sPath.Value);
        if (property == null || property.Value.Type != PropertyType.String)
        {
            parameters.Stack.Push(new ForthDatum(""));
            return ForthProgramResult.SUCCESS;
        }

        parameters.Stack.Push(new ForthDatum((string)property.Value.Value));
        return ForthProgramResult.SUCCESS;
    }
}