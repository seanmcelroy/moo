using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ForthDatum;
using static ForthProgramResult;

public static class StrCut
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRCUT ( s i -- s1 s2 ) 

        Cuts string s after its i'th character. For example,
        "Foobar" 3 strcut returns

        "Foo" "bar" If i is zero or greater than the length of s, returns a null string in the first or second position, respectively.
        */
        if (parameters.Stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "STRCUT requires two parameters");

        var ni = parameters.Stack.Pop();
        if (ni.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRCUT requires the top parameter on the stack to be an integer");

        var sSource = parameters.Stack.Pop();
        if (sSource.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRCUT requires the second-to-top parameter on the stack to be a string");

        var index = (int)ni.Value;
        var source = (string)sSource.Value;

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
            parameters.Stack.Push(new ForthDatum(source.Substring(0, index)));
            parameters.Stack.Push(new ForthDatum(source.Substring(index)));
        }

        return default(ForthProgramResult);
    }
}