using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class CtoI
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        CTOI ( s -- i ) 

        Converts the first character in s into its ASCII equivilent.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "CTOI requires one parameter");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "CTOI requires the top parameter on the stack to be a string");

        if (n1.Value == null || ((string)n1.Value).Length == 0)
            stack.Push(new ForthDatum(0));
        else
        {
            var ascii = Encoding.ASCII.GetBytes((string)n1.Value);
            stack.Push(new ForthDatum(Convert.ToInt32(ascii[0])));
        }

        return default(ForthProgramResult);
    }
}