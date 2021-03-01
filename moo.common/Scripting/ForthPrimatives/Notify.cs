using System.Threading.Tasks;
using static ForthDatum;

public static class Notify
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        NOTIFY ( d s -- ) 

        d must be a player object. s must be a string. Tells player d message s. If s is null it will print nothing. 
        This primitive will trigger the _listen'er property on the object the message is sent to,
        unless the program that would be run is the same as one one currently running.
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "NOTIFY requires two parameters");

        var sMessage = parameters.Stack.Pop();
        if (sMessage.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NOTIFY requires the top parameter on the stack to be a string");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NOTIFY requires the second-to-top parameter on the stack to be a dbref");

        if (sTarget.UnwrapDbref().ToInt32() < 0)
            return ForthPrimativeResult.SUCCESS;

        var message = (string?)sMessage.Value;
        if (message == null || string.IsNullOrWhiteSpace(message))
            return ForthPrimativeResult.SUCCESS;

        await Server.GetInstance().NotifyAsync(sTarget.UnwrapDbref(), message);

        return ForthPrimativeResult.SUCCESS;
    }
}