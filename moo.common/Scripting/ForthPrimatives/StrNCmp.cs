using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ForthDatum;
using static ForthProgramResult;

public static class StrNCmp
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        STRNCMP ( s1 s2 i -- i' ) 

        Compares the first i characters in strings s1 and s2. Return value is like strcmp.
        */
        if (stack.Count < 3)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "STRNCMP requires three parameters");

        var ni = stack.Pop();
        if (ni.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRNCMP requires the top parameter on the stack to be an integer");

        var n2 = stack.Pop();
        if (n2.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRNCMP requires the second-to-top parameter on the stack to be a string");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRNCMP requires the third-to-top parameter on the stack to be a string");

        var si = (int)n1.Value;
        var s1 = (string)n1.Value;
        var s2 = (string)n2.Value;

        for (var n = 0; n < Math.Max(s1.Length, s2.Length) && n < si; n++)
        {
            if (n > s1.Length - 1)
            {
                stack.Push(new ForthDatum((int)Encoding.ASCII.GetBytes(s2[n].ToString())[0]));
                return default(ForthProgramResult);
            }
            else if (n > s2.Length - 1)
            {
                stack.Push(new ForthDatum(-1 * (int)Encoding.ASCII.GetBytes(s1[n].ToString())[0]));
                return default(ForthProgramResult);
            }
            else
            {
                var c1 = (int)Encoding.ASCII.GetBytes(s1[n].ToString())[0];
                var c2 = (int)Encoding.ASCII.GetBytes(s2[n].ToString())[0];
                if (c1 != c2)
                {
                    stack.Push(new ForthDatum(c2 - c1));
                    return default(ForthProgramResult);
                }
            }
        }

        stack.Push(new ForthDatum(0));
        return default(ForthProgramResult);
    }
}