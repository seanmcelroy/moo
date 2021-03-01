using static ForthDatum;

public static class OpIsInt
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        INT? ( x -- i ) 

        Returns true if x is a int.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "INT? requires at least one parameter");

        var n1 = parameters.Stack.Pop();

        parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.Integer ? 1 : 0));
        return ForthPrimativeResult.SUCCESS;
    }
}