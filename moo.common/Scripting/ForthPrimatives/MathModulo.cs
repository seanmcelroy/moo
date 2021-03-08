using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class MathModulo
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            % ( n1 n2 -- i ) 

            This returns the integer modulo (remainder) of the division of two numbers, n1 % n2.
            Floats cannot use the % modulo function. For them, use either the FMOD or MODF primitives.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "% requires two parameters");

            var n2 = parameters.Stack.Pop();
            var n1 = parameters.Stack.Pop();

            if (n2.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "% requires arguments to be integers");

            if (n1.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "% requires arguments to be integers");

            parameters.Stack.Push(new ForthDatum(n1.UnwrapInt() % n2.UnwrapInt()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}