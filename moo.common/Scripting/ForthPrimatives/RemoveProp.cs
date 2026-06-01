using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class RemoveProp
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            REMOVE_PROP ( d s -- )

            Removes property s from object d. If the property begins with an underscore, `_' or a dot `.', and the effective user does not have permission on that object, the call fails.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "REMOVE_PROP requires four parameters");

            var s = parameters.Stack.Pop();
            if (s.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "REMOVE_PROP requires the to-top parameter on the stack to be a string");

            var d = parameters.Stack.Pop();
            if (d.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "REMOVE_PROP requires the second-top parameter on the stack to be a dbref");

            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(d.UnwrapDbref(), parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
                return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {d.UnwrapDbref()}");

            var path = ((string?)s.Value) ?? string.Empty;
            targetResult.value.ClearPropertyPath(path);
            return ForthPrimativeResult.SUCCESS;
        }
    }
}