using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class Descriptors
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        DESCRIPTORS (d -- ix...i1 i) 

        Takes a player dbref, or #-1, and returns the range of descriptor numbers associated with that dbref (or all for #-1) with their count on top. Descriptors are numbers that always stay the same for a connection, while a connection# is the relative position in the WHO list of a connection.
        Also see: DESCRCON and CONDESCR
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DESCRIPTORS requires two parameters");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DESCRIPTORS requires the top parameter on the stack to be a dbref");

        var connectionDescriptors = parameters.Server.GetConnectionDescriptors((Dbref)n1.Value).ToArray();

        foreach (var connectionDescriptor in connectionDescriptors)
            parameters.Stack.Push(new ForthDatum(connectionDescriptor));

        parameters.Stack.Push(new ForthDatum(connectionDescriptors.Length));
        return default(ForthProgramResult);
    }
}