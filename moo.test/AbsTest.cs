using System.Collections.Generic;
using System.Threading;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;

namespace Tests
{
    public class AbsTest
    {
        [Test]
        public void Positive()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = Abs.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(42, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void Negative()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(-42));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = Abs.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(42, n.Value);

            Assert.AreEqual(0, local.Count);
        }
    }
}