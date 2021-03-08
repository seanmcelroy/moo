using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Location
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            LOCATION ( d -- d' ) 

            Returns location of object d as object d'.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "LOCATION requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "LOCATION requires the top parameter on the stack to be a dbref");

            var target = n1.UnwrapDbref();
            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(target, parameters.CancellationToken);

            if (!targetResult.isSuccess || targetResult.value == null)
            {
                parameters.Stack.Push(new ForthDatum(Dbref.NOT_FOUND, 0));
                return ForthPrimativeResult.SUCCESS;
            }

            parameters.Stack.Push(new ForthDatum(targetResult.value.Location, 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}