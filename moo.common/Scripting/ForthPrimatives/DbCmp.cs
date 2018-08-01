using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class DbCmp
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        DBCMP ( d1 d2 -- i ) 

        Performs comparison of database objects d1 and d2. If they are the same object, then i is 1, otherwise i is 0. Use of this primitive is deprecated, as dbrefs may be compared with the standard comparison functions (< <= > >= =).
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DBCMP requires two parameters");

        var n2 = stack.Pop();
        if (n2.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DBCMP requires the second-to-top parameter on the stack to be a dbref");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DBCMP requires the top parameter on the stack to be a dbref");

        stack.Push(new ForthDatum(((Dbref)n1.Value) == ((Dbref)n2.Value) ? 1 : 0));
        return default(ForthProgramResult);
    }
}