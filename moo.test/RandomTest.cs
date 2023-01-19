using System.Collections.Generic;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;

namespace Tests
{
    public class RandomTest
    {
        [Test]
        public void GenerateUnseededRandomNumber()
        {
            var local = new Stack<ForthDatum>();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = RandomMethods.Random(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.AreEqual(1, local.Count);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.Greater(n.UnwrapInt(), 0);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void GenerateSeededRandomNumber()
        {
            var local = new Stack<ForthDatum>();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = RandomMethods.SRand(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.AreEqual(1, local.Count);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.Greater(n.UnwrapInt(), 0);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void GetSeed()
        {
            var local = new Stack<ForthDatum>();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = RandomMethods.GetSeed(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.AreEqual(1, local.Count);

            var seed = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, seed.Type);
            Assert.NotNull(seed.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void SetSeed()
        {
            var localSetSeed1 = new Stack<ForthDatum>(new[] { new ForthDatum("unit-test-seed") });
            var setSeed1Parameters = new ForthPrimativeParameters(null, localSetSeed1, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var setSeed1Result = RandomMethods.SetSeed(setSeed1Parameters);

            var getRandom1Stack = new Stack<ForthDatum>();
            var getRandom1Parameters = new ForthPrimativeParameters(null, getRandom1Stack, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var getRandom1 = RandomMethods.SRand(getRandom1Parameters);
            var getRandom1Value = getRandom1Stack.Pop().UnwrapInt();

            var localGetSeed = new Stack<ForthDatum>();
            var getSeedParameters = new ForthPrimativeParameters(null, localGetSeed, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var getSeed = RandomMethods.GetSeed(getSeedParameters);
            var getSeedValue = localGetSeed.Pop();

            Assert.AreEqual("unit-test-seed", getSeedValue.Value);

            var localSetSeed2 = new Stack<ForthDatum>(new[] { new ForthDatum("unit-test-seed") });
            var setSeed2Parameters = new ForthPrimativeParameters(null, localSetSeed2, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var setSeed2Result = RandomMethods.SetSeed(setSeed2Parameters);

            var getRandom2Stack = new Stack<ForthDatum>();
            var getRandom2Parameters = new ForthPrimativeParameters(null, getRandom2Stack, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var getRandom2 = RandomMethods.SRand(getRandom2Parameters);
            var getRandom2Value = getRandom2Stack.Pop().UnwrapInt();

            Assert.AreEqual(getRandom1Value, getRandom2Value, "When setting the same seed, the same random numbers should be returned.");

            var getRandom3Stack = new Stack<ForthDatum>();
            var getRandom3Parameters = new ForthPrimativeParameters(null, getRandom3Stack, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var getRandom3 = RandomMethods.SRand(getRandom3Parameters);
            Assert.NotNull(getRandom3);
            Assert.IsTrue(getRandom3.IsSuccessful);

            var getRandom3Value = getRandom3Stack.Pop().UnwrapInt();

            Assert.AreNotEqual(getRandom2Value, getRandom3Value, "Subsequent random numbers should not be the same");
        }
    }
}