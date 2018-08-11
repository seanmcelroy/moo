using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthPrimativeResult;
using static Property;

public static class NotifyExclude
{
    public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
    {
        /*
        NOTIFY_EXCLUDE (d dn ... d1 n s -- ) 

        Displays the message s to all the players (or _listening objects), excluding the n given players, in the given room. For example:
        #0 #1 #23 #7 3 "Hi!" notify_exclude would send "Hi!" to everyone in room #0 except for players (or objects) #1, #7, and #23. _listener's will not be triggered by a notify_exclude if the program they would run is the same as the current program running.
        */
        if (parameters.Stack.Count < 3)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "NOTIFY_EXCLUDE requires at least 3 parameters");

        var sMessage = parameters.Stack.Pop();
        if (sMessage.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NOTIFY_EXCLUDE requires the top parameter on the stack to be an string");

        var iExcludeCount = parameters.Stack.Pop();
        if (iExcludeCount.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NOTIFY_EXCLUDE requires the second-to-top parameter on the stack to be an integer");

        var excludeList = new List<Dbref>();
        for (int i = 0; i < (int)iExcludeCount.Value; i++)
        {
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"NOTIFY_EXCLUDE specified {iExcludeCount.Value} exclusions, but we ran out at position {i}");

            var sExclude = parameters.Stack.Pop();
            if (sExclude.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NOTIFY_EXCLUDE requires the exclude list members to be dbrefs");
            if (((Dbref)sExclude.Value).ToInt32() >= 0)
                excludeList.Add((Dbref)sExclude.Value);
        }

        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"NOTIFY_EXCLUDE did not include a target room dbref");
        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NOTIFY_EXCLUDE requires the target room parameter on the stack to be a dbref");

        if (((Dbref)sTarget.Value).ToInt32() < 0)
            return ForthPrimativeResult.SUCCESS;

        var message = (string)sMessage.Value;
        if (message == null || string.IsNullOrWhiteSpace(message))
            return ForthPrimativeResult.SUCCESS;

        await parameters.NotifyRoom.Invoke(sTarget.UnwrapDbref(), message, excludeList);

        return ForthPrimativeResult.SUCCESS;
    }
}