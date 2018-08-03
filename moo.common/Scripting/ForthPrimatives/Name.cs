using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;

public static class Name
{
    public static async Task<ForthProgramResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        NAME ( d -- s ) 

        Takes object d and returns its name (@name) string field.
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "NAME requires two parameters");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "NAME requires the top parameter on the stack to be a dbref");


        var target = (Dbref)n1.Value;
        var targetResult = await ThingRepository.GetAsync<Thing>(target, parameters.CancellationToken);

        if (!targetResult.isSuccess)
        {
            parameters.Stack.Push(new ForthDatum(""));
            return default(ForthProgramResult);
        }

            parameters.Stack.Push(new ForthDatum(targetResult.value.name));
        return default(ForthProgramResult);
    }
}