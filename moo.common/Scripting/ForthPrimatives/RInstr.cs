using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class RInstr
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            RINSTR ( s s1 -- i ) 

            Returns the last occurrence of string s1 in string s, or 0 if s1 is not found. '"abcbcba" "bc" rinstr' returns 4.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "RINSTR requires two parameters");

            var n2 = parameters.Stack.Pop();
            if (n2.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "RINSTR requires the second-to-top parameter on the stack to be a string");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "RINSTR requires the top parameter on the stack to be a string");

            var s1 = (string?)n1.Value ?? string.Empty;
            var s2 = (string?)n2.Value ?? string.Empty;

            parameters.Stack.Push(new ForthDatum(s1.LastIndexOf(s2) + 1));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}