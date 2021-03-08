using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class StripLead
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            STRIPLEAD (s -- s) 

            Strips leading spaces from the given string.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "TOLOWER requires one parameter");

            var s = parameters.Stack.Pop();
            if (s.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TOLOWER requires the top parameter on the stack to be a string");

            parameters.Stack.Push(new ForthDatum(((string)s.Value).TrimStart()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}