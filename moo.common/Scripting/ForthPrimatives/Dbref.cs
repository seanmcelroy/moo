using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class DbrefConvert
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        DBREF ( i -- d ) 

        Converts integer i to object reference d.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DBREF requires one parameter");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DBREF requires the top parameter on the stack to be an integer");

        var ni = (int)n1.Value;
        if (ni < 0)
            stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
        else
            stack.Push(new ForthDatum(new Dbref(ni, DbrefObjectType.Unknown), 0));
        return default(ForthProgramResult);
    }
}