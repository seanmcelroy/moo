using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;
using static ForthVariable;

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
        result.dirtyVariables = new Dictionary<string, ForthVariable> {
            { svar.Value.ToString().ToLowerInvariant(), new ForthVariable(sval.Value, sval.Type == DatumType.String ? VariableType.String : (sval.Type == DatumType.Float ? VariableType.Float : (sval.Type == DatumType.Integer ? VariableType.Integer : (sval.Type == DatumType.DbRef ? VariableType.DbRef : VariableType.Unknown))), false) }
        };
        return result;
    }
}
