using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class MathInt
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack, Dictionary<string, object> variables, Player player, Dbref trigger, string command)
    {
        /*
        INT ( x -- i ) 
        Converts variable, float, or dbref x to integer i.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "INT requires one parameter");

        ForthDatum datum;

        var reference = stack.Pop();
        if (reference.Type == DatumType.Unknown)
        {
            // Resolve variable 
            var variableName = reference.Value.ToString().ToLowerInvariant();
            datum = At.ResolveVariableByName(variables, player, trigger, command, variableName);

            if (default(ForthDatum).Equals(datum) && !variables.ContainsKey(variableName))
                return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

            if (default(ForthDatum).Equals(datum))
                return new ForthProgramResult(ForthProgramErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for: " + datum.Value);
        }
        else
        {
            datum = reference;
        }

        stack.Push(datum.ToInteger());
        return default(ForthProgramResult);
    }
}