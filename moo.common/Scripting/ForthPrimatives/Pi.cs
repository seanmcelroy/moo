using System;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Pi
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            PI ( -- f )

            Returns the value of Pi. 
            */
            parameters.Stack.Push(new ForthDatum(MathF.PI));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}