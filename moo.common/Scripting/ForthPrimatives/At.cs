using System.Collections.Generic;
using System.Collections.Immutable;
using moo.common.Models;

namespace moo.common.Scripting.ForthPrimatives
{
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

            var variableName = reference.Value?.ToString()?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(variableName))
                return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable name was found");

            var variables = parameters.Variables;
            if (variables == null)
                return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

            var variableValue = ResolveVariableByName(variables, parameters.Player, parameters.Location, parameters.Trigger, parameters.Command, variableName);

            if (default(ForthVariable).Equals(variableValue) && !variables.ContainsKey(variableName))
                return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

            if (!default(ForthVariable).Equals(variableValue))
            {
                parameters.Stack.Push(new ForthDatum(variableValue, reference.FileLineNumber, null, reference.WordName, reference.WordLineNumber));
                return ForthPrimativeResult.SUCCESS;
            }

            return new ForthPrimativeResult(ForthErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for {variableName}: {variableValue.Value}");
        }

        public static ForthVariable ResolveVariableByName(IReadOnlyDictionary<string, ForthVariable> variables, Dbref id, Dbref location, Dbref trigger, string command, string variableName)
        {
            if (variables == null)
                return default;

            // Handle built-in variables.  (Note that "here" is not a built-in supported here.)
            if (string.Compare("me", variableName, System.StringComparison.OrdinalIgnoreCase) == 0)
                return new ForthVariable(id, ForthVariable.VariableType.DbRef, true);

            if (string.Compare("loc", variableName, System.StringComparison.OrdinalIgnoreCase) == 0)
                return new ForthVariable(location, ForthVariable.VariableType.DbRef, true);

            if (string.Compare("trigger", variableName, System.StringComparison.OrdinalIgnoreCase) == 0)
                return new ForthVariable(trigger, ForthVariable.VariableType.DbRef, true);

            if (string.Compare("command", variableName, System.StringComparison.OrdinalIgnoreCase) == 0)
                return new ForthVariable(command, ForthVariable.VariableType.String, true);

            var key = variableName.ToLowerInvariant();
            if (!variables.TryGetValue(key, out var variableValue))
                return default;

            if (variableValue.Value == null)
                return default;

            if (variableValue.Value is Dbref d)
                return new ForthVariable(d, 0);

            if (variableValue.Value is float f)
                return new ForthVariable(f);

            if (variableValue.Value is int i)
                return new ForthVariable(i);

            if (variableValue.Value is string s)
                return new ForthVariable(s);

            if (variableValue.Value is ForthDictionaryArray da)
                return new ForthVariable(da);

            if (variableValue.Value is ForthListArray la)
                return new ForthVariable(la);

            return default;
        }
    }
}