using System.Threading.Tasks;
using moo.common.Database;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class Force
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            FORCE (d s -- )

            Forces player d to do action s as if they were @forced. (wizbit only)
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "FORCE requires two parameters");

            var sAction = parameters.Stack.Pop();
            if (sAction.Type != DatumType.String || sAction.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "FORCE requires the top parameter on the stack to be a string");

            var sVictim = parameters.Stack.Pop();
            if (sVictim.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "FORCE requires the second-to-top parameter on the stack to be a dbref");

            if ((parameters.Process?.EffectiveMuckerLevel ?? 0x00) < 0x4)
                return new ForthPrimativeResult(ForthErrorResult.INSUFFICIENT_PERMISSION, "Wizbit only primitive.");

            var target = sVictim.UnwrapDbref();
            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(target, parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
                return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, "Invalid object to force.");

            if (targetResult.value.Type != Dbref.DbrefObjectType.Player && targetResult.value.Type != Dbref.DbrefObjectType.Thing)
                return new ForthPrimativeResult(ForthErrorResult.INVALID_VALUE, "Object to force not a thing or player.");

            var action = (string)sAction.Value;
            if (action.Contains('\r'))
                return new ForthPrimativeResult(ForthErrorResult.INVALID_VALUE, "Carriage returns not allowed in command string.");


            //if (God(oper2->data.objref) && !God(OWNER(program)))
            //    abort_interp("Cannot force god (1).");

            var conn = Server.GetConnection(target);
            if (conn == null)
                return ForthPrimativeResult.SUCCESS;

            conn.ReceiveInput(action);
            // TODO: force levels to track recursion https://github.com/fuzzball-muck/fuzzball/blob/e8c6e70c91098d8b7ba7f96a10c88b58f34acd1f/src/p_misc.c#L615
            return ForthPrimativeResult.SUCCESS;
        }
    }
}