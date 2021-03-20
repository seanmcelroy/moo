using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Dbref;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Contents
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            CONTENTS ( d -- d' ) 

            Pushes the dbref of the first thing contained by d. This dbref can then be referenced by `next' to cycle through all of the contents of d. d may be a room or a player.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "CONTENTS requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "CONTENTS requires the top parameter on the stack to be a dbref");

            var target = await n1.UnwrapDbref().Get(parameters.CancellationToken);
            if (target == null || (target.Type != DbrefObjectType.Room && target.Type != DbrefObjectType.Player))
            {
                parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
                return ForthPrimativeResult.SUCCESS;
            }

            var container = target;
            var first = container.FirstContent();

            parameters.Stack.Push(new ForthDatum(first, 0));
            return new ForthPrimativeResult("CONTENTS completed", first);
        }
    }
}