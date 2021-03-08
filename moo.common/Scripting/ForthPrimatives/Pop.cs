namespace moo.common.Scripting.ForthPrimatives
{
    public static class Pop
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            // POP ( x -- ) 
            // Pops the top of the stack into oblivion.
            if (parameters.Stack.Count == 0)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "POP requires at least one item on the stack, but the stack is empty.");

            parameters.Stack.Pop();

            return ForthPrimativeResult.SUCCESS;
        }
    }
}