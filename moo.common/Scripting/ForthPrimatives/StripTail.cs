using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class StripTail
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            STRIPTAIL (s -- s) 

            Strips trailing spaces from the given string.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "STRIPTAIL requires one parameter");

            var s = parameters.Stack.Pop();
            if (s.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRIPTAIL requires the top parameter on the stack to be a string");

            parameters.Stack.Push(new ForthDatum(((string?)s.Value ?? string.Empty).TrimEnd()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}