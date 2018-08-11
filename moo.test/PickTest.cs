using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    public class PickTest
    {
        Stack<ForthDatum> stack = new Stack<ForthDatum>();

        [SetUp]
        public void Setup()
        {
            stack.Push(new ForthDatum("a"));
            stack.Push(new ForthDatum("b"));
            stack.Push(new ForthDatum("c"));
            stack.Push(new ForthDatum("d"));
        }

        [Test]
        public void ForwardShift()
        {
            var local = new Stack<ForthDatum>(stack);
            local.Push(new ForthDatum(3));
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Pick.Execute(parameters);
            Assert.AreEqual(default(ForthPrimativeResult), result);

            var b = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, b.Type);
            Assert.AreEqual("b", b.Value);

            Assert.AreEqual(5, local.Count);

            Assert.Pass();
        }
    }
}