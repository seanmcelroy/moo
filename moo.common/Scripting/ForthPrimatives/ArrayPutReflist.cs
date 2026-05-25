using System.Linq;
using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayPutReflist
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_PUT_REFLIST ( d s a -- )

            Takes a list array of dbrefs, and stores them in a property as a space delimited
            string of dbrefs. ie: "#1234 #6646 #1026 #7104" 
            */
            if (parameters.Stack.Count < 3)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_PUT_REFLIST requires three parameters");

            var sArray = parameters.Stack.Pop();
            if (sArray.Type != DatumType.Array || sArray.Value == null || sArray.Value is not ForthListArray la)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_PUT_REFLIST requires the top parameter on the stack to be a list array");

            var array = la;

            if (array.Any(a => a.Type != DatumType.DbRef))
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_PUT_REFLIST requires every element in the top parameter array to be a dbref");

            var sPath = parameters.Stack.Pop();
            if (sPath.Type != DatumType.String || sPath.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_PUT_REFLIST requires the second-top parameter on the stack to be a string");

            var sTarget = parameters.Stack.Pop();
            if (sTarget.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_PUT_REFLIST requires the third-to-top parameter on the stack to be a dbref");

            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
                return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

            var path = ((string?)sPath.Value) ?? string.Empty;
            targetResult.value.SetPropertyPathValue(path, new ForthVariable(sArray));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}