using System.Collections.Generic;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;

namespace Tests
{
    public class PickTest
    {
        [Test]
        public void Dup()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("x"));

            var local = stack.ClonePreservingOrder();
            local.Push(new ForthDatum(1));
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = Pick.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.AreEqual(2, local.Count);

            var x1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, x1.Type);
            Assert.AreEqual("x", x1.Value);

            var x2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, x2.Type);
            Assert.AreEqual("x", x2.Value);
            Assert.AreEqual(x1, x2);
        }

        [Test]
        public void Over()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("x"));
            stack.Push(new ForthDatum("y"));

            var local = stack.ClonePreservingOrder();
            local.Push(new ForthDatum(2));
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = Pick.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.AreEqual(3, local.Count);

            var x1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, x1.Type);
            Assert.AreEqual("x", x1.Value);

            var y = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, y.Type);
            Assert.AreEqual("y", y.Value);

            var x2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, x2.Type);
            Assert.AreEqual("x", x2.Value);
            Assert.AreEqual(x1, x2);
        }

        [Test]
        public void Pick3()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("a"));
            stack.Push(new ForthDatum("b"));
            stack.Push(new ForthDatum("c"));
            stack.Push(new ForthDatum("d"));
            stack.Push(new ForthDatum(3));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = Pick.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.AreEqual(5, local.Count);

            var b1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, b1.Type);
            Assert.AreEqual("b", b1.Value);

            var d = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, d.Type);
            Assert.AreEqual("d", d.Value);

            var c = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, c.Type);
            Assert.AreEqual("c", c.Value);

            var b2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, b2.Type);
            Assert.AreEqual("b", b2.Value);
            Assert.AreEqual(b1, b2);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, a.Type);
            Assert.AreEqual("a", a.Value);
        }
    }
}