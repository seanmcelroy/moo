using System.Collections.Generic;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;

namespace Tests
{
    public class SignTest
    {
        [Test]
        public void Positive()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = Sign.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void Negative()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(-42));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = Sign.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(-1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void Zero()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = Sign.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }
    }
}