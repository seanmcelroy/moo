using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Bang
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        ! ( x v -- ) 
        Sets variable v's value to x.
        */
        if (parameters.Stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "! requires two parameters");

        var svar = parameters.Stack.Pop();
        var sval = parameters.Stack.Pop();

        var result = new ForthProgramResult(null, $"Variable {svar.Value} set to {sval.Value}");
        result.dirtyVariables = new Dictionary<string, object> {
            { svar.Value.ToString().ToLowerInvariant(), sval.Value }
        };
        return result;
    }
}