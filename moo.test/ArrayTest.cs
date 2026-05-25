using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;

namespace moo.Test
{
    [TestClass]
    public class ArrayTest
    {
        [TestMethod]
        public void ArrayCountSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthListArray([
                new ForthDatum("asdf"),
                new ForthDatum(12),
                new ForthDatum(34.56F),
            ])));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayCount.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(3, n.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void ArrayKeysSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthListArray([
                new ForthDatum("value0"),
                new ForthDatum("value1")
            ])));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayKeys.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(2, n.Value);

            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, s2.Type);
            Assert.AreEqual(1, s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, s1.Type);
            Assert.AreEqual(0, s1.Value);

            Assert.HasCount(0, local);
        }

        [TestMethod]
        public void ArrayValsSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthListArray([
                new("value0"),
                new("value1")
            ])));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayVals.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(2, n.Value);

            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("value1", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("value0", s1.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void ArrayMakeSimple()
        {
            // MUF push order convention: the first item pushed becomes element 0
            // ("value1" was pushed first, so it's the deepest item and ends up at arr[0]).
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("value1"));
            stack.Push(new ForthDatum("value0"));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayMake.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            var arr = Assert.IsInstanceOfType<ForthListArray>(a.Value);
            Assert.HasCount(2, arr);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[0].Type);
            Assert.AreEqual("value1", arr[0].Value);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[1].Type);
            Assert.AreEqual("value0", arr[1].Value);

            Assert.HasCount(0, local);
        }

        [TestMethod]
        public void ArrayMakeEmpty()
        {
            // Regression for the original bug where `0 array_make` threw because
            // Aggregate had no seed and was called on an empty sequence.
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayMake.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            var la = Assert.IsInstanceOfType<ForthListArray>(a.Value);
            Assert.HasCount(0, la);
            Assert.HasCount(0, local);
        }

        [TestMethod]
        public void EmptyArrayUnwrapsBackToEmpty()
        {
            // Regression for §3.C: after the constructor fix put "" in Value
            // for an empty array, UnwrapArray was still splitting "" into [""]
            // and indexing kvps[1], throwing IndexOutOfRangeException.
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(0));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var makeResult = ArrayMake.Execute(parameters);
            Assert.IsTrue(makeResult.IsSuccessful, makeResult.Reason);

            var a = local.Pop();
            var unwrapped = a.UnwrapListArray();
            Assert.HasCount(0, unwrapped);
        }

        [TestMethod]
        public void ArrayMakeDictSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("index0"));
            stack.Push(new ForthDatum("value0"));
            stack.Push(new ForthDatum("index1"));
            stack.Push(new ForthDatum("value1"));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayMakeDict.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            Assert.IsInstanceOfType<ForthDictionaryArray>(a.Value);

            var arr = a.UnwrapDictionaryArray();
            Assert.HasCount(2, arr);

            Assert.AreEqual("value0", arr.GetValueOrDefault("index0", default).Value);
            Assert.AreEqual("value1", arr.GetValueOrDefault("index1", default).Value);

            Assert.IsEmpty(local);
        }

        [TestMethod]
        public void ArrayReverseSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthListArray([
                new ForthDatum("value0"),
                new ForthDatum("value1"),
            ])));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayReverse.Execute(parameters);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            Assert.IsInstanceOfType<ForthListArray>(a.Value);

            var arr = a.UnwrapListArray();
            Assert.HasCount(2, arr);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[0].Type);
            Assert.AreEqual("value1", arr[0].Value);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[1].Type);
            Assert.AreEqual("value0", arr[1].Value);

            Assert.IsEmpty(local);
        }
    }
}