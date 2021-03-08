using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Instr
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            INSTR ( s s1 -- i ) 

            Returns the first occurrence of string s1 in string s, or 0 if s1 is not found.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "INSTR requires two parameters");

            var n2 = parameters.Stack.Pop();
            if (n2.Type != DatumType.String || n2.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "INSTR requires the second-to-top parameter on the stack to be a string");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.String || n1.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "INSTR requires the top parameter on the stack to be a string");

            var s1 = (string)n1.Value;
            var s2 = (string)n2.Value;

            parameters.Stack.Push(new ForthDatum(s1.IndexOf(s2) + 1));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}