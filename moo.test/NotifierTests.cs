using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;

namespace moo.Test
{
    [TestClass]
    public class NotifierTests
    {
        [TestMethod]
        public async Task NotifyPrimitiveUsesCallbackWhenProvided()
        {
            // Drive a tiny program through a primitive directly with a captured
            // callback to confirm the routing change. Note: the target dbref
            // must be a positive id; the primitive intentionally short-circuits
            // for negative dbrefs (Dbref.NOT_FOUND is -1) without calling the
            // callback at all, which is correct behavior.
            var captured = new List<(Dbref dbref, string msg)>();
            Task notify(Dbref dbref, string msg)
            {
                captured.Add((dbref, msg));
                return Task.CompletedTask;
            }

            var target = new Dbref("#1");
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(target));
            stack.Push(new ForthDatum("hello"));

            var p = new ForthPrimativeParameters(null, stack, null,
                Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null,
                notify, null, null, null, default);

            var result = await Notify.ExecuteAsync(p);

            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, captured.Count);
            Assert.AreEqual(target, captured[0].dbref);
            Assert.AreEqual("hello", captured[0].msg);
        }
    }
}