using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class Next
{
    public static async Task<ForthProgramResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        NEXT ( d -- d' ) 

        Takes object d and returns the next thing in the linked contents/exits list of d's location.
        */
        if (!parameters.LastListItem.HasValue)
            return new ForthProgramResult(ForthProgramErrorResult.SYNTAX_ERROR, "NEXT invoked without a CONTENTS or EXIT previously invoked");

        if (parameters.LastListItem.Value == Dbref.NOT_FOUND)
            return new ForthProgramResult("NEXT completed early", Dbref.NOT_FOUND);


        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "NEXT requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "NEXT requires the top parameter on the stack to be a dbref");

        var target = (Dbref)n1.Value;
        var targetResult = await ThingRepository.GetAsync<Thing>(target, parameters.CancellationToken);

        if (!targetResult.isSuccess)
        {
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
            return default(ForthProgramResult);
        }

        if (!typeof(Container).IsAssignableFrom(targetResult.value.GetType()))
        {
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
            return default(ForthProgramResult);
        }

        var container = (Container)targetResult.value;
        var next = container.NextContent(parameters.LastListItem.Value, new DbrefObjectType[] { DbrefObjectType.Room, DbrefObjectType.Player });

        parameters.Stack.Push(new ForthDatum(next, 0));
        return new ForthProgramResult("NEXT completed", next);
    }
}