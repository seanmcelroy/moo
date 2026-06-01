using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;

namespace moo.Test
{
    [TestClass]
    public class ArrayJoinTest
    {
        [TestMethod]
        public async Task ArrayJoinTestSimple()
        {
            var testObj = ThingRepository.Instance.Make<Thing>();

            // Set the property up
            {
                var stack = new Stack<ForthDatum>([
                     new ForthDatum(new ForthListArray([
                        new ForthDatum("one"),
                        new ForthDatum(2),
                        new ForthDatum("three"),
                        new ForthDatum(3.14159F)
                    ])),
                    new ForthDatum("... ")
                ]);

                var local = stack.ClonePreservingOrder();
                var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
                var joinResult = ArrayJoin.Execute(parameters);
                Assert.IsTrue(joinResult.IsSuccessful);

                var n = local.Pop();
                Assert.AreEqual(ForthDatum.DatumType.String, n.Type);
                Assert.IsInstanceOfType<string>(n.Value);
                Assert.AreEqual("one... 2... three... 3.14159", n.Value);
            }
        }
    }
}