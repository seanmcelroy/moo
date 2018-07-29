using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class MathDivide
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        / ( n1 n2 -- n ) 

        This divides two numbers, n1 / n2. If both numbers are integers, an integer will be returned. If one of them is a floating
        point number, then a float will be returned. You can also use this on a dbref or a variable number, so long as the
        second argument is an integer. In those cases, this will return a dbref or variable number, respectively.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "* requires two parameters");

        var n2 = stack.Pop();
        var n1 = stack.Pop();

        if (n1.Type == DatumType.Integer && n2.Type == DatumType.Integer)
        {
            var n2v = (int)n2.Value;
            if (n2v == 0)
                return new ForthProgramResult(ForthProgramErrorResult.DIVISION_BY_ZERO, "Attempt to divide by zero was aborted");

            stack.Push(new ForthDatum((int)n1.Value / n2v));
            return default(ForthProgramResult);
        }

        if ((n1.Type == DatumType.Integer || n1.Type == DatumType.Float) &&
            (n2.Type == DatumType.Integer || n2.Type == DatumType.Float))
        {
            var n2v = Convert.ToSingle(n2.Value);
            if (n2v == 0)
                return new ForthProgramResult(ForthProgramErrorResult.DIVISION_BY_ZERO, "Attempt to divide by zero was aborted");

            stack.Push(new ForthDatum(Convert.ToSingle(n1.Value) / n2v));
            return default(ForthProgramResult);
        }

        if ((n1.Type == DatumType.Integer && n2.Type == DatumType.DbRef) ||
           (n1.Type == DatumType.DbRef || n2.Type == DatumType.Integer))
        {
            var n2v = (int)n2.Value;
            if (n2v == 0)
                return new ForthProgramResult(ForthProgramErrorResult.DIVISION_BY_ZERO, "Attempt to divide by zero was aborted");

            stack.Push(new ForthDatum(new Dbref((int)n1.Value / n2v), 0));
            return default(ForthProgramResult);
        }

        return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "/ expects integers or floating point numbers, or no more than one dbref and an integer");

        // TODO: We do not support variable numbers today.  They're depreciated anyway.
    }
}