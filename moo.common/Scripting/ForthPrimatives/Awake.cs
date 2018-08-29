using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class Awake
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        AWAKE? ( d -- i ) 

        Passed a players dbref, returns the number of connections they have to the game. This will be 0 if they are not connected.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "AWAKE? requires two parameters");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "AWAKE? requires the top parameter on the stack to be a dbref");

        var connectionCount = Server.GetInstance().GetConnectionCount(n1.UnwrapDbref());

        parameters.Stack.Push(new ForthDatum(connectionCount));
        return ForthPrimativeResult.SUCCESS;
    }
}