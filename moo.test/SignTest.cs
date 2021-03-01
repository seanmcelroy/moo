using System.Collections.Generic;
using System.Threading;
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
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Sign.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void Negative()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(-42));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Sign.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(-1, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void Zero()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = Sign.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }
    }
}