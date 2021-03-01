using System;
using System.Text;
using static ForthDatum;

public static class StrNCmp
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRNCMP ( s1 s2 i -- i' ) 

        Compares the first i characters in strings s1 and s2. Return value is like strcmp.
        */
        if (parameters.Stack.Count < 3)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "STRNCMP requires three parameters");

        var ni = parameters.Stack.Pop();
        if (ni.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRNCMP requires the top parameter on the stack to be an integer");

        var n2 = parameters.Stack.Pop();
        if (n2.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRNCMP requires the second-to-top parameter on the stack to be a string");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRNCMP requires the third-to-top parameter on the stack to be a string");

        var si = ni.UnwrapInt();
        var s1 = (string?)n1.Value ?? string.Empty;
        var s2 = (string?)n2.Value ?? string.Empty;

        for (var n = 0; n < Math.Max(s1.Length, s2.Length) && n < si; n++)
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