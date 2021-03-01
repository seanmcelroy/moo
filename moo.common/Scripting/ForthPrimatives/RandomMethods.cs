using System;
using static ForthDatum;

public static class RandomMethods
{
    private static string SEED = "default-seed";
    private static Random? _seedRandom;
    private static readonly Random _tickRandom = new System.Random(Environment.TickCount);
    private static readonly object _randomLock = new object();

    private static Random GetSeededRandom()
    {
        lock (_randomLock)
        {
            if (_seedRandom == null)
                SetNewRandomSeed(Environment.TickCount64.ToString());
        }
        return _seedRandom!;
    }

    static void SetNewRandomSeed(string seed)
    {
        if (seed == null)
            seed = "default-seed";

        var textSeedHash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        var newSeedValue = BitConverter.ToInt32(textSeedHash, 0);
        SEED = seed;
        _seedRandom = new Random(newSeedValue);
    }

    public static ForthPrimativeResult SRand(ForthPrimativeParameters parameters)
    {
        /*
        RANDOM ( -- i )

        Returns a random integer from 0 to the MAXINT of the system running the MUCK. In general this number is (2^31)-1 or 2,147,483,647 (2.1 billion). This is based on the standard C random() function, so it's not very secure.
        */
        var randomGenerator = GetSeededRandom();
        var randomNumber = randomGenerator.Next(0, int.MaxValue);
        parameters.Stack.Push(new ForthDatum(randomNumber));

        return ForthPrimativeResult.SUCCESS;
    }

    public static ForthPrimativeResult GetSeed(ForthPrimativeParameters parameters)
    {
        /*
        GETSEED ( -- s )

        Returns the the current SRAND seed string. 
        */
        var randomGenerator = GetSeededRandom(); // Create seed if it doesn't exist.
        parameters.Stack.Push(new ForthDatum(SEED));

        return ForthPrimativeResult.SUCCESS;
    }

    public static ForthPrimativeResult SetSeed(ForthPrimativeParameters parameters)
    {
        /*
        SETSEED ( s -- )

        Sets the seed for SRAND.
        Only the first thirty-two characters are significant.
        If SRAND is called before SETSEED is called, then SRAND is seeded with a semi-random value. 
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "SETSEED requires one parameter");

        var s = parameters.Stack.Pop();
        if (s.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "SETSEED requires the top parameter on the stack to be a string");

        SetNewRandomSeed((string?)s.Value ?? string.Empty);

        return ForthPrimativeResult.SUCCESS;
    }

    public static ForthPrimativeResult Random(ForthPrimativeParameters parameters)
    {
        /*
        RANDOM ( -- i )

        Returns a random integer from 0 to the MAXINT of the system running the MUCK. In general this number is (2^31)-1 or 2,147,483,647 (2.1 billion). This is based on the standard C random() function, so it's not very secure.
        */
        var randomNumber = _tickRandom.Next(0, int.MaxValue);
        parameters.Stack.Push(new ForthDatum(randomNumber));
        return ForthPrimativeResult.SUCCESS;
    }
}