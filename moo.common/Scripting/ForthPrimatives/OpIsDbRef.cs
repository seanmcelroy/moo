using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class OpIsDbRef
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            DBREF? ( x -- i ) 

            Returns true if x is a dbref.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "DBREF? requires one parameter");

            var n1 = parameters.Stack.Pop();

            parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.DbRef ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}