using static ForthDatum;

public static class MathBitShift
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        BITSHIFT (i i -- i)

        Shifts the first integer by the second integer's number of bit positions. Same as the C << operator. If the second integer is negative, its like >>. 
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "BITSHIFT requires two parameters");

        var n2 = parameters.Stack.Pop();
        var n1 = parameters.Stack.Pop();

        if (n2.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "BITSHIFT requires arguments to be integers");

        if (n1.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "BITSHIFT requires arguments to be integers");

        var shiftBy = n2.UnwrapInt();
        var num = n1.UnwrapInt();

        if (shiftBy >= 32)
            parameters.Stack.Push(new ForthDatum(0));
        else if (shiftBy <= -32)
            parameters.Stack.Push(new ForthDatum(num > 0 ? 0 : -1));
        else if (shiftBy == 0)
            parameters.Stack.Push(new ForthDatum(num));
        else if (shiftBy > 0)
            parameters.Stack.Push(new ForthDatum(num << shiftBy));
        else
            parameters.Stack.Push(new ForthDatum(num >> (-shiftBy)));

        return ForthPrimativeResult.SUCCESS;
    }
}