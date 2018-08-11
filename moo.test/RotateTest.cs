using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    public class RotateTest
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
            /*
             Rotates the top i things on the stack. Using a negative rotational value rotates backwards. Examples:
                "a"  "b"  "c"  "d"  4  rotate
            would leave
                "b"  "c"  "d"  "a"
            on the stack.
             */
            var local = new Stack<ForthDatum>(stack);
            local.Push(new ForthDatum(4));
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Rotate.Execute(parameters);
            Assert.AreEqual(default(ForthPrimativeResult), result);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, a.Type);
            Assert.AreEqual("a", a.Value);

            var d = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, d.Type);
            Assert.AreEqual("d", d.Value);

            var c = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, c.Type);
            Assert.AreEqual("c", c.Value);

            var b = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, b.Type);
            Assert.AreEqual("b", b.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }
    }
}