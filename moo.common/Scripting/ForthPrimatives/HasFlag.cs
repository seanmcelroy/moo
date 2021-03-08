using System;
using System.Threading.Tasks;
using moo.common.Models;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class HasFlag
    {
        public static async Task<ForthPrimativeResult> ExecuteAsync(ForthPrimativeParameters parameters)
        {
            /*
            FLAG? ( d s -- i ) 

            Reads the flag of object d, specified by s, and returns its state: 1 = on; 0 = off.
            The ! token may be used before the name of a flag to negate the check and check for the absense of the flag
            Different flags may be supported in different installations. flag? returns 0 for unsupported or unrecognized flags.

            You can check the "interactive" flag to see if a player is currently in a program's READ, or if they are in the MUF editor.
            The "Truewizard" flag will check for a W flag with or without the QUELL set. The "Mucker" flag returns the most significant
            bit of the mucker level and the "Nucker" flag returns the least significant bit. (Use MLEVEL instead.)
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "FLAG? requires two parameters");

            var sFlag = parameters.Stack.Pop();
            if (sFlag.Type != DatumType.String)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "FLAG? requires the top parameter on the stack to be a string");

            var sTarget = parameters.Stack.Pop();
            if (sTarget.Type != DatumType.DbRef)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "FLAG? requires the second-to-top parameter on the stack to be a dbref");

            if (sTarget.UnwrapDbref().ToInt32() < 0)
                return ForthPrimativeResult.SUCCESS;

            var flagString = (string?)sFlag.Value;
            if (flagString == null || string.IsNullOrWhiteSpace(flagString))
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var target = sTarget.UnwrapDbref();
            var targetResult = await ThingRepository.Instance.GetAsync<Thing>(target, parameters.CancellationToken);

            if (!targetResult.isSuccess || targetResult.value == null)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var negate = flagString.StartsWith("!");
            var flagChar = negate ? flagString[1] : flagString[0];
            var flagValue = (Thing.Flag)Enum.ToObject(typeof(Thing.Flag), (UInt16)flagChar.ToString().ToUpperInvariant()[0]);
            if (flagValue == default)
                return new ForthPrimativeResult(ForthErrorResult.SYNTAX_ERROR, $"FLAG? understands no flag named {flagChar}");

            var hasFlag = targetResult.value?.HasFlag(flagValue) == true;
            parameters.Stack.Push(new ForthDatum(hasFlag ^ negate ? 1 : 0));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}