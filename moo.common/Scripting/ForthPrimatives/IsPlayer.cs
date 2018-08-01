using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;
using static Property;

public static class IsPlayer
{
    public static async Task<ForthProgramResult> ExecuteAsync(Stack<ForthDatum> stack, CancellationToken cancellationToken)
    {
        /*
        PLAYER? ( d -- i ) 

        Returns 1 if object d is a player object, otherwise returns 0. If the dbref is that of an invalid object, it will return 0.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "PLAYER? requires one parameter");

        var sTarget = stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "PLAYER? requires the top parameter on the stack to be a dbref");

        if (((Dbref)sTarget.Value).ToInt32() < 0)
        {
            stack.Push(new ForthDatum(0));
            return default(ForthProgramResult);
        }

        var targetResult = await ThingRepository.GetAsync<Thing>(sTarget.UnwrapDbref(), cancellationToken);
        if (!targetResult.isSuccess)
        {
            stack.Push(new ForthDatum(0));
            return default(ForthProgramResult);
        }

        if (!typeof(Player).IsAssignableFrom(targetResult.value.GetType()))
            stack.Push(new ForthDatum(0));

        stack.Push(new ForthDatum(1));
        return default(ForthProgramResult);
    }
}