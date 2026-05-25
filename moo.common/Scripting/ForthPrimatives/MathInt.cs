using System.Runtime.InteropServices;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class MathInt
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            INT ( x -- i ) 
            Converts variable, float, or dbref x to integer i.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "INT requires one parameter");

            ForthDatum datum;

            var reference = parameters.Stack.Pop();
            if (reference.Type == DatumType.Unknown)
            {
                // Resolve variable 
                var variableName = reference.Value?.ToString()?.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(variableName))
                    return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable name was found");

                var variables = parameters.Variables;
                if (variables == null)
                    return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

                var variableValue = At.ResolveVariableByName(parameters.Variables, parameters.Player, parameters.Location, parameters.Trigger, parameters.Command, variableName);

                if (default(ForthVariable).Equals(variableValue) && !variables.ContainsKey(variableName))
                    return new ForthPrimativeResult(ForthErrorResult.VARIABLE_NOT_FOUND, $"No variable named {variableName} was found");

                if (default(ForthVariable).Equals(variableValue))
                    return new ForthPrimativeResult(ForthErrorResult.UNKNOWN_TYPE, $"Unable to determine data type for: {variableValue.Value}");

                datum = new ForthDatum(variableValue);
            }
            else
            {
                datum = reference;
            }

            parameters.Stack.Push(datum.ToInteger());
            return ForthPrimativeResult.SUCCESS;
        }
    }
}