using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class OpIsFloat
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            FLOAT? ( ? -- i ) 

            Returns true if the item on the stack is a floating point value.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "FLOAT? requires at least one parameter");

            var n1 = parameters.Stack.Pop();

            parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.Float ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}