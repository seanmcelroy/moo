using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Split
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            SPLIT ( s1 s2 -- s1' s2' ) 

            Splits string s1 at the first found instance of s2. If there are no matches of s2 in s1, will return s1 and a null string.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SPLIT requires two parameters");

            var s2 = parameters.Stack.Pop();
            if (s2.Type != DatumType.String || s2.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SPLIT requires the top parameter on the stack to be a string");

            var s1 = parameters.Stack.Pop();
            if (s1.Type != DatumType.String || s1.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SPLIT requires the second-to-top parameter on the stack to be a string");

            var str1 = (string)s1.Value;
            var str2 = (string)s2.Value;

            var idx = str1.IndexOf(str2);

            if (idx == -1)
            {
                parameters.Stack.Push(new ForthDatum(str1));
                parameters.Stack.Push(new ForthDatum(""));
                return ForthPrimativeResult.SUCCESS;
            }

            var strA = str1.Substring(0, idx);
            var strB = str1[(idx + 1)..];

            parameters.Stack.Push(new ForthDatum(strA));
            parameters.Stack.Push(new ForthDatum(strB));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}