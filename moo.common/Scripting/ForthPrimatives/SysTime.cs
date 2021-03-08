using System;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class SysTime
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            SYSTIME ( -- i ) 

            Returns the number of seconds from Jan 1, 1970 GMT. This is compatible with the system timestamps
            and may be broken down into useful values through 'timesplit'.
            */
            parameters.Stack.Push(new ForthDatum((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

            return ForthPrimativeResult.SUCCESS;
        }
    }
}