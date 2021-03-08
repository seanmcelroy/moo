using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Subst
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            SUBST ( s1 s2 s3 -- s ) 

            s1 is the string to operate on, s2 is the string to change all occurrences of s3 into, and s is resultant string. For example:
                "HEY_YOU_THIS_IS" " " "_" subst
            results in
                "HEY YOU THIS IS"
            s2 and s3 may be of any length.
            */
            if (parameters.Stack.Count < 3)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SUBST requires three parameters");

            var s3 = parameters.Stack.Pop();
            if (s3.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SUBST requires the top parameter on the stack to be a string");

            var s2 = parameters.Stack.Pop();
            if (s2.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SUBST requires the second-to-top parameter on the stack to be a string");

            var s1 = parameters.Stack.Pop();
            if (s1.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SUBST requires the third-to-top parameter on the stack to be a string");

            var result = (s3.Value == null)
                ? (string?)s1.Value ?? string.Empty
                : ((string?)s1.Value ?? string.Empty).Replace((string)s3.Value, (string?)s2.Value ?? string.Empty);

            parameters.Stack.Push(new ForthDatum(result));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}