using System.Collections.Generic;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void DivideByStringReturnsTypeMismatchInsteadOfThrowing()
        {
            // Bug: MathDivide's DbRef branch used `||` instead of `&&`. With a
            // (String, Integer) pair on the stack, the condition was true (because
            // n2 was Integer), so the branch entered and called n1.UnwrapDbref()
            // on a String, which threw InvalidCastException. The fix narrows the
            // branch to require both operands match.
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("hello"));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathDivide.Execute(parameters);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ForthErrorResult.TYPE_MISMATCH, result.Result);
        }

        [TestMethod]
        public void DivideFloatByFloatStillWorks()
        {
            // Guard against re-regressing the float branch. (An earlier attempted
            // fix accidentally turned the inner `||`s into `&&`s, making this
            // condition unsatisfiable and breaking all float division.)
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10.0F));
            stack.Push(new ForthDatum(4.0F));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathDivide.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var top = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Float, top.Type);
            Assert.AreEqual(2.5F, top.Value);
        }

        [TestMethod]
        public void DivideDbrefByIntegerStillWorks()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new Dbref("#10")));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathDivide.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var top = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.DbRef, top.Type);
            Assert.IsNotNull(top.Value);
            Assert.AreEqual(5, ((Dbref)top.Value).ToInt32());
        }

        [TestMethod]
        public void ModuloByZeroReturnsDivisionByZero()
        {
            // Bug: MathModulo had no zero check and would throw
            // DivideByZeroException at runtime. The fix added a guard that
            // returns ForthErrorResult.DIVISION_BY_ZERO, matching MathDivide.
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10));
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathModulo.Execute(parameters);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ForthErrorResult.DIVISION_BY_ZERO, result.Result);
        }

        [TestMethod]
        public void ModuloNonZeroStillWorks()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(10));
            stack.Push(new ForthDatum(3));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = MathModulo.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var top = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, top.Type);
            Assert.AreEqual(1, top.Value);
        }
    }
}
