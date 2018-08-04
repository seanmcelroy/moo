using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class At
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        @ ( v -- x ) 
        Retrieves variable v's value x.
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "@ requires one parameter");

        var reference = parameters.Stack.Pop();
        if (reference.Type != DatumType.Variable)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "@ requires the top parameter on the stack to be a variable");

        var variableName = reference.Value.ToString().ToLowerInvariant();
        var variableValue = ResolveVariableByName(parameters.Variables, parameters.Connection.Dbref, parameters.Connection.Location, parameters.Trigger, parameters.Command, variableName);

        if (default(ForthDatum).Equals(variableValue) && !parameters.Variables.ContainsKey(variableName))
            return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

        if (!default(ForthDatum).Equals(variableValue))
        {
            parameters.Stack.Push(variableValue);
            return default(ForthProgramResult);
        }

        return new ForthProgramResult(ForthProgramErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for {variableName}: {variableValue.Value}");
    }

    public static ForthDatum ResolveVariableByName(Dictionary<string, ForthVariable> variables, Dbref id, Dbref location, Dbref trigger, string command, string variableName)
    {
        // Handle built-in variables.
        if (string.Compare("me", variableName) == 0)
            return new ForthDatum(id, DatumType.DbRef);

        if (string.Compare("loc", variableName) == 0)
            return new ForthDatum(location, DatumType.DbRef);

        if (string.Compare("trigger", variableName) == 0)
            return new ForthDatum(trigger, DatumType.DbRef);

        if (string.Compare("command", variableName) == 0)
            return new ForthDatum(command, DatumType.String);

        if (!variables.ContainsKey(variableName))
            return default(ForthDatum);

        var variableValue = variables[variableName];
        if (variableValue.Value == null)
            return default(ForthDatum);

        if (variableValue.Value.GetType() == typeof(float))
            return new ForthDatum((float?)variableValue.Value);

        if (variableValue.Value.GetType() == typeof(int))
            return new ForthDatum((int?)variableValue.Value);

        if (variableValue.Value.GetType() == typeof(string))
        {
            return new ForthDatum((string)variableValue.Value);
        }

        return default(ForthDatum);
    }
}