using System.Collections.Generic;
using System.Threading.Tasks;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    [TestClass]
    public class BitwiseTests
    {
        [TestMethod]
        public void BitwiseOr()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));
            stack.Push(new ForthDatum(91));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitOr.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(123, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void BitwiseXOr()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));
            stack.Push(new ForthDatum(91));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitXOr.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(113, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void BitwiseAnd()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(42));
            stack.Push(new ForthDatum(91));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitAnd.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(10, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public async Task BitwiseAndDispatchedToBitAndNotBitXor()
        {
            // Regression for the original bug where callTable["bitand"] was wired
            // to MathBitXOr.Execute, so `42 91 bitand` silently computed XOR (113)
            // instead of AND (10). A direct unit test of MathBitAnd would not have
            // caught this because the dispatch table is what was wrong; exercise the
            // primitive via the engine's name lookup.
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(": main 42 91 bitand ;");
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            var top = stack.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, top.Type);
            Assert.AreEqual(10, top.Value); // 42 & 91 = 10  (XOR would give 113)
        }

        [TestMethod]
        public void BitshiftLeft2()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitShift.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(196, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void BitshiftRight2()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(-2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitShift.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(12, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void BitshiftLeftTooFar()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(33));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitShift.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void BitshiftRightTooFar()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(-32));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitShift.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void BitshiftZero()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(49));
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathBitShift.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(49, n.Value);

            Assert.AreEqual(0, local.Count);
        }
    }
}