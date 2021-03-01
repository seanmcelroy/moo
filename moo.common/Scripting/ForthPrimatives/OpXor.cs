public static class OpXor
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        XOR ( x1 x2 -- i ) 

        Returns true (1) if either of the top two stack items are considered true, but NOT both of them. Returns false (0) otherwise. The stack items can be of any type. For the various types, here are their false values:
            Integer      0
            Float        0.0
            DBRef        #-1
            String       ""
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "XOR requires two parameters");

        var n1 = parameters.Stack.Pop();
        var n2 = parameters.Stack.Pop();

        parameters.Stack.Push(new ForthDatum(n1.isTrue() ^ n2.isTrue() ? 1 : 0));
        return ForthPrimativeResult.SUCCESS;
    }
}