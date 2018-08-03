using System;
using System.Collections.Generic;
using System.Linq;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class DescRcon
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        DESCRCON (i -- i) 

        Takes a descriptor and returns the associated connection number, or 0 if no match was found. (Requires Mucker Level 3)
        Also see: DESCRIPTORS and CONDESCR
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DESCRCON requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DESCRCON requires the top parameter on the stack to be an integer");

        var connectionNumber = parameters.Server.GetConnectionNumberForConnectionDescriptor((int)n1.Value);
        parameters.Stack.Push(new ForthDatum(connectionNumber ?? 0));
        return default(ForthProgramResult);
    }
}