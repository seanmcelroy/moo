using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ForthDatum;
using static ForthPrimativeResult;

public static class StrCmp
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRCMP ( s1 s2 -- i ) 

        Compares strings s1 and s2. Returns i as 0 if they are equal,
        otherwise returns i as the difference between the first non-matching character in the strings.

        For example, "z" "a" strcmp returns 25. The reason it returns a 0 for a match, and the difference on a non-match,
        is to allow for nice things like string sorting functions. This primitive is case sensitive, unlike stringcmp.
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "STRCMP requires two parameters");

        var n2 = parameters.Stack.Pop();
        if (n2.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRCMP requires the second-to-top parameter on the stack to be a string");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRCMP requires the top parameter on the stack to be a string");

        var s1 = (string)n1.Value;
        var s2 = (string)n2.Value;

        for (var n = 0; n < Math.Max(s1.Length, s2.Length); n++)
        {
            if (n > s1.Length - 1)
            {
                parameters.Stack.Push(new ForthDatum((int)Encoding.ASCII.GetBytes(s2[n].ToString())[0]));
                return ForthPrimativeResult.SUCCESS;
            }
            else if (n > s2.Length - 1)
            {
                parameters.Stack.Push(new ForthDatum(-1 * (int)Encoding.ASCII.GetBytes(s1[n].ToString())[0]));
                return ForthPrimativeResult.SUCCESS;
            }
            else
            {
                var c1 = (int)Encoding.ASCII.GetBytes(s1[n].ToString())[0];
                var c2 = (int)Encoding.ASCII.GetBytes(s2[n].ToString())[0];
                if (c1 != c2)
                {
                    parameters.Stack.Push(new ForthDatum(c2 - c1));
                    return ForthPrimativeResult.SUCCESS;
                }
            }
        }

        parameters.Stack.Push(new ForthDatum(0));
        return ForthPrimativeResult.SUCCESS;
    }
}