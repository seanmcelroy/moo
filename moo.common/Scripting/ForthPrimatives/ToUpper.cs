using static ForthDatum;

public static class ToUpper
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        TOUPPER (s -- s) 

        Takes a string and returns it with all the letters in uppercase.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "TOUPPER requires one parameter");

        var s = parameters.Stack.Pop();
        if (s.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TOUPPER requires the top parameter on the stack to be a string");

        parameters.Stack.Push(new ForthDatum(((string)s.Value).ToUpperInvariant()));
        return ForthPrimativeResult.SUCCESS;
    }
}