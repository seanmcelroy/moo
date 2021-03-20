using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class IsProgram
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            PROGRAM? ( d -- i ) 

            Returns 1 if object d is a program, otherwise returns 0. If the dbref is that of an invalid object, it will return 0.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PROGRAM? requires one parameter");

            var sTarget = parameters.Stack.Pop();
            if (sTarget.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PROGRAM? requires the top parameter on the stack to be a dbref");

            if (sTarget.UnwrapDbref().ToInt32() < 0)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var target = await sTarget.UnwrapDbref().Get(parameters.CancellationToken);
            if (target == null)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            if (target.type != (int)Dbref.DbrefObjectType.Program)
                parameters.Stack.Push(new ForthDatum(0));
            else
                parameters.Stack.Push(new ForthDatum(1));

            return ForthPrimativeResult.SUCCESS;
        }
    }
}