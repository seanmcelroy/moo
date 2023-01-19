using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class StrCut
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            STRCUT ( s i -- s1 s2 ) 

            Cuts string s after its i'th character. For example,
            "Foobar" 3 strcut returns

            "Foo" "bar" If i is zero or greater than the length of s, returns a null string in the first or second position, respectively.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "STRCUT requires two parameters");

            var ni = parameters.Stack.Pop();
            if (ni.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRCUT requires the top parameter on the stack to be an integer");

            var sSource = parameters.Stack.Pop();
            if (sSource.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRCUT requires the second-to-top parameter on the stack to be a string");

            var index = ni.UnwrapInt();
            var source = (string?)sSource.Value ?? string.Empty;

            if (index == 0)
            {
                parameters.Stack.Push(new ForthDatum(""));
                parameters.Stack.Push(new ForthDatum(source));
            }
            else if (index > source.Length)
            {
                parameters.Stack.Push(new ForthDatum(source));
                parameters.Stack.Push(new ForthDatum(""));
            }
            else
            {
                parameters.Stack.Push(new ForthDatum(source[..index]));
                parameters.Stack.Push(new ForthDatum(source[index..]));
            }

            return ForthPrimativeResult.SUCCESS;
        }
    }
}