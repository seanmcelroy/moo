using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class IsOk
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            OK? ( x -- i ) 

            Takes x and returns 1 if x is a type dbref, as well as 0 or above, below the top of the database, and is not an object of type garbage.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "OK? requires one parameter");

            var x = parameters.Stack.Pop();
            if (x.Type != DatumType.DbRef)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var xValue = x.UnwrapDbref();
            if (xValue.ToInt32() < 0)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(xValue, parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            parameters.Stack.Push(new ForthDatum(1));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}