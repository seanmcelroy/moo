using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class MathInt
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        INT ( x -- i ) 
        Converts variable, float, or dbref x to integer i.
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "INT requires one parameter");

        ForthDatum datum;

        var reference = parameters.Stack.Pop();
        if (reference.Type == DatumType.Unknown)
        {
            // Resolve variable 
            var variableName = reference.Value.ToString().ToLowerInvariant();
            datum = At.ResolveVariableByName(parameters.Variables, parameters.Connection.Dbref, parameters.Connection.Location, parameters.Trigger, parameters.Command, variableName);

            if (default(ForthDatum).Equals(datum) && !parameters.Variables.ContainsKey(variableName))
                return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

            if (default(ForthDatum).Equals(datum))
                return new ForthProgramResult(ForthProgramErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for: " + datum.Value);
        }
        else
        {
            datum = reference;
        }

        parameters.Stack.Push(datum.ToInteger());
        return default(ForthProgramResult);
    }
}