using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Property;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class GetPropFVal
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            GETPROPFVAL ( d s -- f ) 

            Returns the float value stored in the property
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "GETPROPFVAL requires two parameters");

            var sPath = parameters.Stack.Pop();
            if (sPath.Type != DatumType.String || sPath.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETPROPFVAL requires the top parameter on the stack to be a string");

            var sTarget = parameters.Stack.Pop();
            if (sTarget.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETPROPFVAL requires the second-to-top parameter on the stack to be a dbref");

            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
                return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

            var property = await targetResult.value.GetPropertyPathValueAsync((string)sPath.Value, parameters.CancellationToken);
            if (property.Equals(default(Property)) || property.Type != PropertyType.Float)
            {
                parameters.Stack.Push(new ForthDatum(0f));
                return ForthPrimativeResult.SUCCESS;
            }

            parameters.Stack.Push(new ForthDatum((float?)property.Value));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}