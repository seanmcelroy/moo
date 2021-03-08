using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Pennies
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            PENNIES ( d -- i ) 

            Gets the amount of pennies player object d has, or the penny value of thing d.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PENNIES requires one parameter");

            var d = parameters.Stack.Pop();
            if (d.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PENNIES requires the top parameter on the stack to be a dbref");

            var target = d.UnwrapDbref();
            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(target, parameters.CancellationToken);

            if (!targetResult.isSuccess || targetResult.value == null)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            parameters.Stack.Push(new ForthDatum(targetResult.value.pennies));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}