using System.Collections.Generic;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace moo.Test
{
    [TestClass]
    public class ArrayGetReflistTest
    {
        [TestMethod]
        public async Task ArrayPutGetReflistTestSimple()
        {
            var testObj = ThingRepository.Instance.Make<Thing>();

            // Set the property up
            {
                var stack = new Stack<ForthDatum>([
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName"),
                    new ForthDatum(new ForthListArray([
                        new ForthDatum(new Dbref("#1234")),
                        new ForthDatum(new Dbref("#6646")),
                        new ForthDatum(new Dbref("#1026")),
                        new ForthDatum(new Dbref("#7104"))
                    ])),
                ]);
                var parameters = new ForthPrimativeParameters(null, stack.ClonePreservingOrder(), null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
                var putResult = await ArrayPutReflist.ExecuteAsync(parameters);
                Assert.IsTrue(putResult.IsSuccessful);
                Assert.HasCount(1, testObj.properties);
                Assert.IsTrue(testObj.properties.ContainsKey("propName"));
                var prop = testObj.properties["propName"];
                Assert.AreEqual("propName", prop.Name);
                Assert.AreEqual(Property.PropertyType.Array, prop.Type);
                Assert.IsNotNull(prop.value);
                Assert.IsInstanceOfType<string>(prop.value);
            }

            // Now get
            {
                var stack = new Stack<ForthDatum>([
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName")
                ]);

                var local = stack.ClonePreservingOrder();
                var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
                var result = await ArrayGetReflist.ExecuteAsync(parameters);
                Assert.IsTrue(result.IsSuccessful, result.Reason);
                Assert.HasCount(1, local);

                var pop = local.Pop();
                Assert.AreEqual(ForthDatum.DatumType.Array, pop.Type);
                var arr = pop.UnwrapListArray();
                Assert.HasCount(4, arr);
            }
        }
    }
}