using System.Collections.Generic;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;

namespace Tests
{
    public class OpTest
    {
        [Test]
        public void IsArrayTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthDatum[] {
                new ForthDatum("value0"),
                new ForthDatum("value1"),
            }));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsArray.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsArrayFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("string"));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsArray.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsDbrefTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new Dbref("#123")));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsDbRef.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsDbrefFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("string"));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsDbRef.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(12.34F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsFloat.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("string"));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsFloat.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(123));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsInt.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("string"));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsInt.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsStringTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("actual string"));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsString.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsStringFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(123));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpIsString.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntLessThanTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(1));
            stack.Push(new ForthDatum(10));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThan.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntLessThanFalseObvious()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10));
            stack.Push(new ForthDatum(1));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThan.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntLessThanFalseEqual()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(1));
            stack.Push(new ForthDatum(1));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThan.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatLessThanTrue()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(1.0F));
            stack.Push(new ForthDatum(10.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThan.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatLessThanFalseObvious()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10.0F));
            stack.Push(new ForthDatum(1.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThan.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatLessThanFalseEqual()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(100.0F / 50.0F));
            stack.Push(new ForthDatum(10.0F / 5.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThan.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        // LTE
        [Test]
        public void IsIntLessThanOrEqualToTrueObvious()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(1));
            stack.Push(new ForthDatum(10));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThanOrEqual.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntLessThanOrEqualToFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10));
            stack.Push(new ForthDatum(1));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThanOrEqual.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsIntLessThanOrEqualToTrueEqual()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(1));
            stack.Push(new ForthDatum(1));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThanOrEqual.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatLessThanOrEqualToTrueObvious()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(1.0F));
            stack.Push(new ForthDatum(10.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThanOrEqual.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatLessThanOrEqualToFalse()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10.0F));
            stack.Push(new ForthDatum(1.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThanOrEqual.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(0, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [Test]
        public void IsFloatLessThanOrEqualToTrueEqual()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(100.0F / 50.0F));
            stack.Push(new ForthDatum(10.0F / 5.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = OpLessThanOrEqual.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(1, n.Value);

            Assert.AreEqual(0, local.Count);
        }
    }
}