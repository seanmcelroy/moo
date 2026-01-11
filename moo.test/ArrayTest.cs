using System.Collections.Generic;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ArrayTest
    {
        [TestMethod]
        public void ArrayCountSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthDatum[] {
                new ForthDatum("asdf"),
                new ForthDatum(12),
                new ForthDatum(34.56F),
            }));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayCount.Execute(parameters);
            Assert.IsNotNull(result);
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
            stack.Push(new ForthDatum(new ForthDatum[] {
                new ForthDatum("value0"),
                new ForthDatum("value1")
            }));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayKeys.Execute(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var n = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n.Type);
            Assert.AreEqual(2, n.Value);

            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("index1", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("index0", s1.Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void ArrayValsSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthDatum[] {
                new ForthDatum("value0"),
                new ForthDatum("value1")
            }));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayVals.Execute(parameters);
            Assert.IsNotNull(result);
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
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("value1"));
            stack.Push(new ForthDatum("value0"));
            stack.Push(new ForthDatum(2));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayMake.Execute(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            Assert.IsInstanceOfType(a.Value, typeof(string));

            var arr = a.UnwrapArray();
            Assert.IsNotNull(arr);
            Assert.AreEqual(2, arr.Length);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[0].Type);
            Assert.AreEqual("value0", arr[0].Value);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[1].Type);
            Assert.AreEqual("value1", arr[1].Value);

            Assert.AreEqual(0, local.Count);
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
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            Assert.IsInstanceOfType(a.Value, typeof(string));

            var arr = a.UnwrapArray();
            Assert.IsNotNull(arr);
            Assert.AreEqual(2, arr.Length);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[0].Type);
            Assert.AreEqual("index0", arr[0].Key);
            Assert.AreEqual("value0", arr[0].Value);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[1].Type);
            Assert.AreEqual("index1", arr[1].Key);
            Assert.AreEqual("value1", arr[1].Value);

            Assert.AreEqual(0, local.Count);
        }

        [TestMethod]
        public void ArrayReverseSimple()
        {
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(new ForthDatum[] {
                new ForthDatum("value0"),
                new ForthDatum("value1"),
            }));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
            var result = ArrayReverse.Execute(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            var a = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Array, a.Type);
            Assert.IsNotNull(a.Value);
            Assert.IsInstanceOfType(a.Value, typeof(string));

            var arr = a.UnwrapArray();
            Assert.IsNotNull(arr);
            Assert.AreEqual(2, arr.Length);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[0].Type);
            Assert.AreEqual("value1", arr[0].Value);

            Assert.AreEqual(ForthDatum.DatumType.String, arr[1].Type);
            Assert.AreEqual("value0", arr[1].Value);

            Assert.AreEqual(0, local.Count);
        }
    }
}