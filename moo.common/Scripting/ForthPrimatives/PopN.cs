using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class PopN
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            // POPN ( ?n..?1 i -- ) 
            // Pops the top i stack items.
            if (parameters.Stack.Count == 0)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "POPN requires at least one parameter");

            var si = parameters.Stack.Pop();
            if (si.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "POPN requires the top parameter on the stack to be an integer");

            int i = si.UnwrapInt();
            if (parameters.Stack.Count < i)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"POPN would have removed {i} items on the stack, but only {parameters.Stack.Count} were present.");

            for (int n = 0; n < i; n++)
                parameters.Stack.Pop();

            return ForthPrimativeResult.SUCCESS;
        }
    }
}