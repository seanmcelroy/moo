public static class Version
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        VERSION ( -- s) 

        Returns the version of this code in a string. "Muck2.2fb5.55", currently.
        */

        parameters.Stack.Push(new ForthDatum("Moo0.1"));
        return ForthPrimativeResult.SUCCESS;
    }
}