using System;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Time
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            TIME ( -- s m h ) 

            Returns the time of day as integers on the stack, seconds, then minutes, then hours.
            */
            var now = DateTime.Now;

            parameters.Stack.Push(new ForthDatum(now.Second));
            parameters.Stack.Push(new ForthDatum(now.Minute));
            parameters.Stack.Push(new ForthDatum(now.Hour));

            return ForthPrimativeResult.SUCCESS;
        }
    }
}