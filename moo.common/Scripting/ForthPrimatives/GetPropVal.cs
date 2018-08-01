using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;
using static Property;

public static class GetPropVal
{
    public static async Task<ForthProgramResult> ExecuteAsync(Stack<ForthDatum> stack, CancellationToken cancellationToken)
    {
        /*
        GETPROPVAL ( d s -- i ) 

        s must be a string. Retrieves the integer value i associated with property s in object d. If the property is cleared, 0 is returned.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "GETPROPVAL requires two parameters");

        var sPath = stack.Pop();
        if (sPath.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the top parameter on the stack to be an string");

        var sTarget = stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the second-to-top parameter on the stack to be a dbref");

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), cancellationToken);
        if (!targetResult.isSuccess)
        {
            stack.Push(new ForthDatum(0));
            return default(ForthProgramResult);
        }

        var property = targetResult.value.GetPropertyPathValue((string)sPath.Value);
        if (property == null || property.Value.Type != PropertyType.Integer)
        {
            stack.Push(new ForthDatum(0));
            return default(ForthProgramResult);
        }

        if (property.Value.Value.GetType() == typeof(long))
            stack.Push(new ForthDatum(Convert.ToInt32((long)property.Value.Value)));
        else
            stack.Push(new ForthDatum((int)property.Value.Value));

        return default(ForthProgramResult);
    }
}