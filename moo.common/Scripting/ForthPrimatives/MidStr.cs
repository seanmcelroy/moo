using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class MidStr
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            MIDSTR ( s i1 i2 -- s ) 

            Returns the substring of i2 characters, starting with character i1. i1 and i2 must both be positive.
            The first character of the string is considered position 1. ie:
            "testing" 2 3 midstr will return the value "est".
            */
            if (parameters.Stack.Count < 3)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "MIDSTR requires three parameters");

            var i2 = parameters.Stack.Pop();
            if (i2.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "MIDSTR requires the top parameter on the stack to be an integer");

            var i1 = parameters.Stack.Pop();
            if (i1.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "MIDSTR requires the second-to-top parameter on the stack to be an integer");

            var s = parameters.Stack.Pop();
            if (s.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "MIDSTR requires the third-to-top parameter on the stack to be a string");

            var str = (string?)s.Value ?? string.Empty;
            var start = i1.UnwrapInt() - 1;
            var length = i2.UnwrapInt();

            if (start < 0 || length <= 0 || start + length > str.Length - 1)
            {
                parameters.Stack.Push(new ForthDatum(""));
                return ForthPrimativeResult.SUCCESS;
            }

            var ret = str.Substring(start, length);
            parameters.Stack.Push(new ForthDatum(ret));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}