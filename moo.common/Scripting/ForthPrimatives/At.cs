using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class At
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack, Dictionary<string, object> variables, PlayerConnection connection, Dbref trigger, string command)
    {
        /*
        @ ( v -- x ) 
        Retrieves variable v's value x.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "@ requires one parameter");

        var reference = stack.Pop();
        if (reference.Type != DatumType.Variable)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "@ requires the top parameter on the stack to be a variable");

        var variableName = reference.Value.ToString().ToLowerInvariant();
        var variableValue = ResolveVariableByName(variables, connection.Dbref, connection.Location, trigger, command, variableName);

        if (default(ForthDatum).Equals(variableValue) && !variables.ContainsKey(variableName))
            return new ForthProgramResult(ForthProgramErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

        if (!default(ForthDatum).Equals(variableValue))
        {
            stack.Push(variableValue);
            return default(ForthProgramResult);
        }

        return new ForthProgramResult(ForthProgramErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for: " + variableValue.Value);
    }

    public static ForthDatum ResolveVariableByName(Dictionary<string, object> variables, Dbref id, Dbref location, Dbref trigger, string command, string variableName)
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
        if (variableValue == null)
            return default(ForthDatum);

        if (variableValue.GetType() == typeof(float))
            return new ForthDatum((float?)variableValue);

        if (variableValue.GetType() == typeof(int))
            return new ForthDatum((int?)variableValue);

        if (variableValue.GetType() == typeof(string))
        {
            return new ForthDatum((string)variableValue);
        }

        return default(ForthDatum);
    }
}