using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Sign
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            SIGN ( i -- i )

            Given an integer, returns 1 if positive, -1 if negative, and 0 if 0.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SIGN requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SIGN requires the top parameter on the stack to be an integer");

            var i = n1.UnwrapInt();
            if (i < 0)
                parameters.Stack.Push(new ForthDatum(-1));
            else if (i > 0)
                parameters.Stack.Push(new ForthDatum(1));
            else
                parameters.Stack.Push(new ForthDatum(0));

            return ForthPrimativeResult.SUCCESS;
        }
    }
}