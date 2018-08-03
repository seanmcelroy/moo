using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public static class Location
{
    public static async Task<ForthProgramResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        LOCATION ( d -- d' ) 

        Returns location of object d as object d'.
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "LOCATION requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "LOCATION requires the top parameter on the stack to be a dbref");

        var target = (Dbref)n1.Value;
        var targetResult = await ThingRepository.GetAsync<Thing>(target, parameters.CancellationToken);

        if (!targetResult.isSuccess)
        {
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
            return default(ForthProgramResult);
        }

        parameters.Stack.Push(new ForthDatum(targetResult.value.location, 0));
        return default(ForthProgramResult);
    }
}