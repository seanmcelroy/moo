using System.Threading.Tasks;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayGetItem
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_GETITEM ( a @ -- ? )

            Returns item with index @ from an array.
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_GETITEM requires two parameters");

            var sIndex = parameters.Stack.Pop();
            if (
                    (sIndex.Type != DatumType.Integer
                    && sIndex.Type != DatumType.String) 
                || sIndex.Value == null)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_GETITEM requires the top parameter on the stack to be an index");

            var sArray = parameters.Stack.Pop();
            if (sArray.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_GETITEM requires the second-to-top parameter on the stack to be an array");

            if (sArray.Value == null)
            {
                parameters.Stack.Push(new ForthDatum((string?)null));
                return ForthPrimativeResult.SUCCESS;
            }

            var value = sArray.Value switch
            {
                ForthDictionaryArray da => da[sIndex.Value],
                ForthListArray la => la[(int)sIndex.Value],
                _ => new ForthDatum(),
            };

            parameters.Stack.Push(value);
            return ForthPrimativeResult.SUCCESS;
        }
    }
}