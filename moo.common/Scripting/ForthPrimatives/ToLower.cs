using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ToLower
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            TOLOWER (s -- s) 

            Takes a string and returns it with all the letters in lowercase.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "TOLOWER requires one parameter");

            var s = parameters.Stack.Pop();
            if (s.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TOLOWER requires the top parameter on the stack to be a string");

            parameters.Stack.Push(new ForthDatum(((string?)s.Value ?? string.Empty).ToLowerInvariant()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}