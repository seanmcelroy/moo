using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Dbref;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Next
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            NEXT ( d -- d' ) 

            Takes object d and returns the next thing in the linked contents/exits list of d's location.
            */
            if (!parameters.LastListItem.HasValue)
                return new ForthPrimativeResult(ForthErrorResult.SYNTAX_ERROR, "NEXT invoked without a CONTENTS or EXIT previously invoked");

            if (parameters.LastListItem.Value == Dbref.NOT_FOUND)
                return new ForthPrimativeResult("NEXT completed early", Dbref.NOT_FOUND);


            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "NEXT requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "NEXT requires the top parameter on the stack to be a dbref");

            var target = n1.UnwrapDbref();
            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(target, parameters.CancellationToken);

            if (!targetResult.isSuccess || targetResult.value == null || (targetResult.value.Type != DbrefObjectType.Room && targetResult.value.Type != DbrefObjectType.Player))
            {
                parameters.Stack.Push(new ForthDatum(NOT_FOUND, 0));
                return ForthPrimativeResult.SUCCESS;
            }

            var container = targetResult.value;
            var next = container.NextContent(parameters.LastListItem.Value);

            parameters.Stack.Push(new ForthDatum(next, 0));
            return new ForthPrimativeResult("NEXT completed", next);
        }
    }
}