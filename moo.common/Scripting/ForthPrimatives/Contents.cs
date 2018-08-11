using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class Contents
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        CONTENTS ( d -- d' ) 

        Pushes the dbref of the first thing contained by d. This dbref can then be referenced by `next' to cycle through all of the contents of d. d may be a room or a player.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "CONTENTS requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "CONTENTS requires the top parameter on the stack to be a dbref");

        var target = (Dbref)n1.Value;
        var targetResult = await ThingRepository.GetAsync<Thing>(target, parameters.CancellationToken);

        if (!targetResult.isSuccess)
        {
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
            return ForthPrimativeResult.SUCCESS;
        }

        if (!typeof(Container).IsAssignableFrom(targetResult.value.GetType()))
        {
            parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
            return ForthPrimativeResult.SUCCESS;
        }

        var container = (Container)targetResult.value;
        var first = container.FirstContent(new DbrefObjectType[] { DbrefObjectType.Room, DbrefObjectType.Player });

        parameters.Stack.Push(new ForthDatum(first, 0));
        return new ForthPrimativeResult("CONTENTS completed", first);
    }
}