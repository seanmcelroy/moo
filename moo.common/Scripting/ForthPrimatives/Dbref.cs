using static Dbref;
using static ForthDatum;

public static class DbrefConvert
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        DBREF ( i -- d ) 

        Converts integer i to object reference d.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "DBREF requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "DBREF requires the top parameter on the stack to be an integer");

        var ni = n1.UnwrapInt();
        if (ni < 0)
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
        else
            parameters.Stack.Push(new ForthDatum(new Dbref(ni, DbrefObjectType.Unknown), 0));
        return ForthPrimativeResult.SUCCESS;
    }
}