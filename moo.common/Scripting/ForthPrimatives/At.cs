using System;
using System.Collections.Generic;
using System.Linq;
using static ForthPrimativeResult;

public static class At
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        @ ( v -- x ) 
        Retrieves variable v's value x.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "@ requires one parameter");

        var reference = parameters.Stack.Pop();
        if (reference.Type != ForthDatum.DatumType.Variable)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "@ requires the top parameter on the stack to be a variable");

        var variableName = reference.Value.ToString().ToLowerInvariant();
        var variableValue = ResolveVariableByName(parameters.Variables, parameters.Connection.Dbref, parameters.Connection.Location, parameters.Trigger, parameters.Command, variableName);

        if (default(ForthVariable).Equals(variableValue) && !parameters.Variables.ContainsKey(variableName))
            return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

        if (!default(ForthVariable).Equals(variableValue))
        {
            parameters.Stack.Push(new ForthDatum(variableValue));
            return ForthPrimativeResult.SUCCESS;
        }

        return new ForthPrimativeResult(ForthErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for {variableName}: {variableValue.Value}");
    }

    public static ForthVariable ResolveVariableByName(Dictionary<string, ForthVariable> variables, Dbref id, Dbref location, Dbref trigger, string command, string variableName)
    {
        // Handle built-in variables.
        if (string.Compare("me", variableName) == 0)
            return new ForthVariable(id, ForthVariable.VariableType.DbRef, true);

        if (string.Compare("loc", variableName) == 0)
            return new ForthVariable(location, ForthVariable.VariableType.DbRef, true);

        if (string.Compare("trigger", variableName) == 0)
            return new ForthVariable(trigger, ForthVariable.VariableType.DbRef, true);

        if (string.Compare("command", variableName) == 0)
            return new ForthVariable(command, ForthVariable.VariableType.String, true);

        if (!variables.ContainsKey(variableName))
            return default(ForthVariable);

        var variableValue = variables[variableName];
        if (variableValue.Value == null)
            return default(ForthVariable);

        if (variableValue.Value.GetType() == typeof(float))
            return new ForthVariable((float?)variableValue.Value);

        if (variableValue.Value.GetType() == typeof(int))
            return new ForthVariable((int?)variableValue.Value);

        if (variableValue.Value.GetType() == typeof(string))
        {
            return new ForthVariable((string)variableValue.Value);
        }

        return default(ForthVariable);
    }
}