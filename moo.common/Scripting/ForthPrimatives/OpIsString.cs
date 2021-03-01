using static ForthDatum;

public static class OpIsString
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRING? ( x -- i ) 

        Returns true if x is a string.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "STRING? requires at least one parameter");

        var n1 = parameters.Stack.Pop();

        parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.String ? 1 : 0));
        return ForthPrimativeResult.SUCCESS;
    }
}