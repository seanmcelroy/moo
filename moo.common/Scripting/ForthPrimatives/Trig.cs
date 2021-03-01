public static class Trig
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        TRIG ( -- d) 

        Returns the dbref of the original trigger.
        */
        parameters.Stack.Push(new ForthDatum(parameters.Trigger, 0));
        return ForthPrimativeResult.SUCCESS;
    }
}