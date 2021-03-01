using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    public class RotateTest
    {
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
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("a"));
            stack.Push(new ForthDatum("b"));
            stack.Push(new ForthDatum("c"));
            stack.Push(new ForthDatum("d"));
            stack.Push(new ForthDatum(4));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Rotate.Execute(parameters);

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

        [Test]
        public void BackwardShift()
        {
            /*
             Rotates the top i things on the stack. Using a negative rotational value rotates backwards. Examples:
                "a"  "b"  "c"  "d"  -4  rotate
            would leave
                "d" "a" "b" "c"
            on the stack.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("a"));
            stack.Push(new ForthDatum("b"));
            stack.Push(new ForthDatum("c"));
            stack.Push(new ForthDatum("d"));
            stack.Push(new ForthDatum(-4));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Rotate.Execute(parameters);

            var c = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, c.Type);
            Assert.AreEqual("c", c.Value);

            var b = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, b.Type);
            Assert.AreEqual("b", b.Value);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, a.Type);
            Assert.AreEqual("a", a.Value);

            var d = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, d.Type);
            Assert.AreEqual("d", d.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void Rotate3()
        {
            /*
             ROT ( x y z -- y z x )

             Rotates the top three things on the stack. This is equivalent to 3 rotate. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("x"));
            stack.Push(new ForthDatum("y"));
            stack.Push(new ForthDatum("z"));

            var local = stack.ClonePreservingOrder();
            local.Push(new ForthDatum(3));
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Rotate.Execute(parameters);

            var x = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, x.Type);
            Assert.AreEqual("x", x.Value);

            var z = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, z.Type);
            Assert.AreEqual("z", z.Value);

            var y = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, y.Type);
            Assert.AreEqual("y", y.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }


    }
}