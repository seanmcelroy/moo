using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;
using static Property;

public static class Notify
{
    public static async Task<ForthProgramResult> ExecuteAsync(Action<Dbref, string> notifyAction, Stack<ForthDatum> stack, CancellationToken cancellationToken)
    {
        /*
        NOTIFY ( d s -- ) 

        d must be a player object. s must be a string. Tells player d message s. If s is null it will print nothing. 
        This primitive will trigger the _listen'er property on the object the message is sent to,
        unless the program that would be run is the same as one one currently running.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "NOTIFY requires two parameters");

        var sMessage = stack.Pop();
        if (sMessage.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "NOTIFY requires the top parameter on the stack to be an string");

        var sTarget = stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "NOTIFY requires the second-to-top parameter on the stack to be a dbref");

        if (((Dbref)sTarget.Value).ToInt32() < 0)
            return default(ForthProgramResult);

        var message = (string)sMessage.Value;
        if (message == null || string.IsNullOrWhiteSpace(message))
            return default(ForthProgramResult);

        notifyAction.Invoke(sTarget.UnwrapDbref(), message);

        return default(ForthProgramResult);
    }
}