using System.Collections.Generic;
using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Models.Property;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayGetReflist
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_GET_REFLIST ( d s -- a )

            Reads in list of space delimited dbrefs from a string property, and returns them as a list array of dbrefs. See ARRAY_PUT_REFLIST for property syntax. 
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_GET_REFLIST requires two parameters");

            var sPath = parameters.Stack.Pop();
            if (sPath.Type != DatumType.String || sPath.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_GET_REFLIST requires the top parameter on the stack to be a string");

            var sTarget = parameters.Stack.Pop();
            if (sTarget.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_GET_REFLIST requires the second-to-top parameter on the stack to be a dbref");

            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(sTarget.UnwrapDbref(), parameters.CancellationToken);
            if (!targetResult.isSuccess || targetResult.value == null)
                return new ForthPrimativeResult(ForthErrorResult.NO_SUCH_OBJECT, $"Unable to find object with dbref {sTarget.UnwrapDbref()}");

            var property = await targetResult.value.GetPropertyPathValueAsync((string)sPath.Value, parameters.CancellationToken);
            if (property.Equals(default(Property)) || property.Type != PropertyType.Array || property.Value == null)
            {
                parameters.Stack.Push(new ForthDatum(ForthListArray.EMPTY, sTarget.FileLineNumber, null, sTarget.WordName, sTarget.WordLineNumber));
                return ForthPrimativeResult.SUCCESS;
            }

            var arrayList = new List<ForthDatum>();

            var split = ((string)property.Value)[1..^1].Split('\x1e', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
            foreach (var s in split)
            {
                if (Dbref.TryParse(s, out Dbref d))
                    arrayList.Add(new ForthDatum(d));
                else
                    return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, $"ARRAY_GET_REFLIST could not parse value '{s}' as a dbref");
            }

            var list = new ForthListArray(arrayList);
            parameters.Stack.Push(new ForthDatum(list));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}