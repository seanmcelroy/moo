using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class Awake
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        AWAKE? ( d -- i ) 

        Passed a players dbref, returns the number of connections they have to the game. This will be 0 if they are not connected.
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "AWAKE? requires two parameters");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "AWAKE? requires the top parameter on the stack to be a dbref");

        var connectionCount = parameters.Server.GetConnectionCount((Dbref)n1.Value);

        parameters.Stack.Push(new ForthDatum(connectionCount));
        return default(ForthProgramResult);
    }
}