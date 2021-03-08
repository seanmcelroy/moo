using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class IntoStr
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            INTOSTR ( x -- s ) 

            x must be an integer or a dbref. Converts x into string s.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "INTOSTR requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Integer && n1.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "< requires the top parameter on the stack to be a number");

            parameters.Stack.Push(new ForthDatum(n1.Value.ToString()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}