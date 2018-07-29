using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Bang
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack, Dictionary<string, object> variables, Player player, int trigger, string command)
    {
        /*
        ! ( x v -- ) 
        Sets variable v's value to x.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "! requires at least two parameters on the stack");

        var svar = stack.Pop();
        var sval = stack.Pop();

        var result = new ForthProgramResult(null, $"Variable {svar.Value} set to {sval.Value}");
        result.dirtyVariables = new Dictionary<string, object> {
            { svar.Value.ToString().ToLowerInvariant(), sval.Value }
        };
        return result;
    }
}