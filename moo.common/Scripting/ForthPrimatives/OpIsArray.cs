using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class OpIsArray
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY? ( ? -- i )

            Tests if stack item is an array. Returns i as 1 if so, 0 if otherwise.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY? requires one parameter");

            var n1 = parameters.Stack.Pop();

            parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.Array ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}