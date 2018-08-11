using System;
using System.Collections.Generic;
using System.Linq;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class MathSubtract
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        - ( n1 n2 -- i ) 

        This subtracts two numbers, n1 - n2. If both numbers are integers, an integer will be returned. If one of them is a
        floating point number, then a float will be returned. You can also use this on a dbref or a variable number, so long as
        the second argument is an integer. In those cases, this will return a dbref or variable number, respectively.
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "- requires two parameters");

        var n2 = parameters.Stack.Pop();
        var n1 = parameters.Stack.Pop();

        if (n1.Type == DatumType.Integer && n2.Type == DatumType.Integer)
        {
            parameters.Stack.Push(new ForthDatum((int)n1.Value - (int)n2.Value));
            return ForthPrimativeResult.SUCCESS;
        }

        if ((n1.Type == DatumType.Integer || n1.Type == DatumType.Float) &&
            (n2.Type == DatumType.Integer || n2.Type == DatumType.Float))
        {
            parameters.Stack.Push(new ForthDatum(Convert.ToSingle(n1.Value) - Convert.ToSingle(n2.Value)));
            return ForthPrimativeResult.SUCCESS;
        }

        if (n1.Type == DatumType.DbRef || n2.Type == DatumType.Integer)
        {
            var n1v = ((Dbref)n1.Value).ToInt32();

            var n2v = (int)n2.Value;
            if (n2v == 0)
                return new ForthPrimativeResult(ForthErrorResult.DIVISION_BY_ZERO, "Attempt to divide by zero was aborted");

            parameters.Stack.Push(new ForthDatum(new Dbref(n1v - n2v, DbrefObjectType.Thing), 0));
            return ForthPrimativeResult.SUCCESS;
        }

        return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "- expects integers or floating point numbers, or no more than one dbref and an integer");

        // TODO: We do not support variable numbers today.  They're depreciated anyway.
    }
}