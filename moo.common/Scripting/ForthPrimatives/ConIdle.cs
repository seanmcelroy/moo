using System;
using System.Collections.Generic;
using System.Linq;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class ConIdle
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        CONIDLE (i -- i) 

        Returns how many seconds the connection has been idle. (Requires Mucker Level 3)
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "CONIDLE requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "CONIDLE requires the top parameter on the stack to be an integer");

        var connection = parameters.Server.GetConnectionForConnectionNumber(n1.UnwrapInt());
        parameters.Stack.Push(new ForthDatum(Convert.ToInt32((DateTime.Now - connection.ConnectionTime).TotalSeconds)));
        return ForthPrimativeResult.SUCCESS;
    }
}