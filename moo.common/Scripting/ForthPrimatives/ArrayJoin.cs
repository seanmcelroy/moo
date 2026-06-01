using System;
using System.Collections.Generic;
using System.Linq;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayJoin
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_JOIN ([s] s -- s)

            Takes a list array and a delimiter string, and returns a single string that is the concatenation of all the items in the list array, with the delimiter string in between. For example:

                { "one" 2 "three" 3.14159 }list "... " array_join

            will result in a single string: "one... 2... three... 3.14159"

            Also see: }JOIN, ARRAY_INTERPRET and }CAT 
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_JOIN requires two parameters");

            var delimParameter = parameters.Stack.Pop();
            if (delimParameter.Type != DatumType.String || delimParameter.Value is not string delim)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_JOIN requires the top parameter on the stack to be a string");

            var arrParameter = parameters.Stack.Pop();
            if (arrParameter.Type != DatumType.Array || arrParameter.Value is not ForthListArray la)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_JOIN requires the second-top parameter on the stack to be a list array");


            var ret = string.Join(delim, la.Select(d => d.Value?.ToString()));
            parameters.Stack.Push(new ForthDatum(ret));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}