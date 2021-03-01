using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    public class BitwiseTests
    {
        [Test]
        public void BitwiseOr()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));
            stack.Push(new ForthDatum(91));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitOr.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(123, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitwiseXOr()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));
            stack.Push(new ForthDatum(91));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitXOr.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(113, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitwiseAnd()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));
            stack.Push(new ForthDatum(91));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitAnd.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(10, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitshiftLeft2()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitShift.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(196, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitshiftRight2()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(-2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitShift.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(12, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitshiftLeftTooFar()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(33));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitShift.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitshiftRightTooFar()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(-32));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitShift.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }

        [Test]
        public void BitshiftZero()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default(CancellationToken));
            var result = MathBitShift.Execute(parameters);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(49, n.Value);

            Assert.AreEqual(0, local.Count);

            Assert.Pass();
        }
    }
}