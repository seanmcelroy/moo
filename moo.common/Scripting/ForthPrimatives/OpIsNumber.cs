using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class OpIsNumber
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            NUMBER? ( s -- i )

            Returns 1 if string on top of the stack contains a number. Otherwise returns 0. 
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "NUMBER? requires one parameter");

            var n1 = parameters.Stack.Pop();

            parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.Float || n1.Type == DatumType.Integer ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}