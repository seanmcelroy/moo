using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class MathBitAnd
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            BITOR (i i -- i)

            Does a mathematical bitwise or.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "BITAND requires two parameters");

            var n2 = parameters.Stack.Pop();
            var n1 = parameters.Stack.Pop();

            if (n2.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "BITAND requires arguments to be integers");

            if (n1.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "BITAND requires arguments to be integers");

            parameters.Stack.Push(new ForthDatum(n1.UnwrapInt() & n2.UnwrapInt()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}