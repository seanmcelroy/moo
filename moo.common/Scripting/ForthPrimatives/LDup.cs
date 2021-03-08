namespace moo.common.Scripting.ForthPrimatives
{
    public static class LDup
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            // LDUP ( {?} -- {?} {?} ) 
            // Duplicates a stackrange on top of the stack.
            foreach (var source in parameters.Stack.ToArray())
                parameters.Stack.Push(source);

            return ForthPrimativeResult.SUCCESS;
        }
    }
}