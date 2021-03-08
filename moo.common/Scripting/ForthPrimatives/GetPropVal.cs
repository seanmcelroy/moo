using System;
using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Property;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class GetPropVal
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            GETPROPVAL ( d s -- i ) 

            s must be a string. Retrieves the integer value i associated with property s in object d. If the property is cleared, 0 is returned.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "GETPROPVAL requires two parameters");

            var sPath = parameters.Stack.Pop();
            if (sPath.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the top parameter on the stack to be a string");

            var sTarget = parameters.Stack.Pop();
            if (sTarget.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "GETPROPVAL requires the second-to-top parameter on the stack to be a dbref");

            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
                return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

            var property = await targetResult.value.GetPropertyPathValueAsync((string)sPath.Value, parameters.CancellationToken);
            if (property.Equals(default(Property)) || property.Type != PropertyType.Integer)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            if (property.Value.GetType() == typeof(long))
                parameters.Stack.Push(new ForthDatum(Convert.ToInt32((long)property.Value)));
            else
                parameters.Stack.Push(new ForthDatum((int)property.Value));

            return ForthPrimativeResult.SUCCESS;
        }
    }
}