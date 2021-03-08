using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class DbCmp
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            DBCMP ( d1 d2 -- i ) 

            Performs comparison of database objects d1 and d2. If they are the same object, then i is 1, otherwise i is 0. Use of this primitive is deprecated, as dbrefs may be compared with the standard comparison functions (< <= > >= =).
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "DBCMP requires two parameters");

            var n2 = parameters.Stack.Pop();
            if (n2.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "DBCMP requires the second-to-top parameter on the stack to be a dbref");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "DBCMP requires the top parameter on the stack to be a dbref");

            parameters.Stack.Push(new ForthDatum(n1.UnwrapDbref() == n2.UnwrapDbref() ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}