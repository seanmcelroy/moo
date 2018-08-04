using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ForthDatum;
using static ForthProgramResult;
using static Property;

public static class HasFlag
{
    public static async Task<ForthProgramResult> ExecuteAsync(ForthPrimativeParameters parameters)
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
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "FLAG? requires two parameters");

        var sFlag = parameters.Stack.Pop();
        if (sFlag.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "FLAG? requires the top parameter on the stack to be an string");

        var sTarget = parameters.Stack.Pop();
        if (sTarget.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "FLAG? requires the second-to-top parameter on the stack to be a dbref");

        if (((Dbref)sTarget.Value).ToInt32() < 0)
            return default(ForthProgramResult);

        var flag = (string)sFlag.Value;
        if (flag == null || string.IsNullOrWhiteSpace(flag))
        {
            parameters.Stack.Push(new ForthDatum(0));
            return default(ForthProgramResult);
        }

        var target = (Dbref)sTarget.Value;
        var targetResult = await ThingRepository.GetAsync<Thing>(target, parameters.CancellationToken);

         if (!targetResult.isSuccess)
        {
            parameters.Stack.Push(new ForthDatum(0));
            return default(ForthProgramResult);
        }

        var negate = flag.StartsWith("!");
        var hasFlag = targetResult.value.HasFlag(negate ? flag.Substring(1) : flag);
        parameters.Stack.Push(new ForthDatum(hasFlag ^ negate ? 1 : 0));
        return default(ForthProgramResult);
    }
}