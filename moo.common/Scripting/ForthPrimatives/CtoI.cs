using System;
using System.Text;
using static ForthDatum;

public static class CtoI
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        CTOI ( s -- i ) 

        Converts the first character in s into its ASCII equivilent.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "CTOI requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "CTOI requires the top parameter on the stack to be a string");

        if (n1.Value == null || ((string)n1.Value).Length == 0)
            parameters.Stack.Push(new ForthDatum(0));
        else
        {
            var ascii = Encoding.ASCII.GetBytes((string)n1.Value);
            parameters.Stack.Push(new ForthDatum(Convert.ToInt32(ascii[0])));
        }

        return ForthPrimativeResult.SUCCESS;
    }
}