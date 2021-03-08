using System.Collections.Generic;
using System.Threading.Tasks;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;
using static moo.common.Models.Property;
using static moo.common.Scripting.ForthDatum;

namespace Tests
{
    public class GetPropTest
    {
        [Test]
        public async Task GetProp_String()
        {
            /*
            GETPROP (d s -- ?)

            Gets the value of a given property, and puts it on the stack.
            This can return a lock, a string, a dbref, or an integer, depending on the type of the property.
            Permissions are the same as those for GETPROPSTR. This primitive returns 0 if no such property exists,
            of if it is a valueless propdir.
            */

            var testObj = ThingRepository.Instance.Make<Thing>();

            // Set the property up
            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName"),
                    new ForthDatum("propValue1"),
                    new ForthDatum(123)
                });
                var parameters = new ForthPrimativeParameters(null, stack.ClonePreservingOrder(), null, null, Dbref.NOT_FOUND, null, null, null, null, default);
                var result1 = await AddProp.ExecuteAsync(parameters);
                Assert.NotNull(result1);
                Assert.IsTrue(result1.IsSuccessful);

                Assert.NotNull(testObj.properties);
                Assert.AreEqual(1, testObj.properties.Count);
                Assert.IsTrue(testObj.properties.ContainsKey("propName"));
                var prop = testObj.properties["propName"];
                Assert.NotNull(prop);
                Assert.AreEqual("propName", prop.Name);
                Assert.AreEqual(PropertyType.String, prop.Type);
                Assert.AreEqual("propValue1", prop.Value);
            }

            // Now get
            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName")
                });

                var local = stack.ClonePreservingOrder();
                var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
                var result = await GetProp.ExecuteAsync(parameters);
                Assert.NotNull(result);
                Assert.IsTrue(result.IsSuccessful);

                Assert.AreEqual(1, local.Count);
                var pop = local.Pop();
                Assert.AreEqual(DatumType.String, pop.Type);
                Assert.AreEqual("propValue1", pop.Value);
            }

            Assert.Pass();
        }

        [Test]
        public async Task GetPropStr_String()
        {
            /*
            GETPROPSTR ( d s -- s )

            s must be a string. Retrieves string associated with property s in object d.
            If the property is cleared, "" (null string) is returned. 
            */

            var testObj = ThingRepository.Instance.Make<Thing>();

            // Set the property up
            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName"),
                    new ForthDatum("propValue1"),
                    new ForthDatum(123)
                });
                var parameters = new ForthPrimativeParameters(null, stack.ClonePreservingOrder(), null, null, Dbref.NOT_FOUND, null, null, null, null, default);
                var result1 = await AddProp.ExecuteAsync(parameters);
                Assert.NotNull(result1);
                Assert.IsTrue(result1.IsSuccessful);

                Assert.NotNull(testObj.properties);
                Assert.AreEqual(1, testObj.properties.Count);
                Assert.IsTrue(testObj.properties.ContainsKey("propName"));
                var prop = testObj.properties["propName"];
                Assert.NotNull(prop);
                Assert.AreEqual("propName", prop.Name);
                Assert.AreEqual(PropertyType.String, prop.Type);
                Assert.AreEqual("propValue1", prop.Value);
            }

            // Now get
            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName")
                });

                var local = stack.ClonePreservingOrder();
                var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
                var result = await GetPropStr.ExecuteAsync(parameters);
                Assert.NotNull(result);
                Assert.IsTrue(result.IsSuccessful);

                Assert.AreEqual(1, local.Count);
                var pop = local.Pop();
                Assert.AreEqual(DatumType.String, pop.Type);
                Assert.AreEqual("propValue1", pop.Value);
            }

            Assert.Pass();
        }

        [Test]
        public async Task GetPropStr_DoesNotExist()
        {
            /*
            GETPROPSTR ( d s -- s )

            s must be a string. Retrieves string associated with property s in object d.
            If the property is cleared, "" (null string) is returned. 
            */

            var testObj = ThingRepository.Instance.Make<Thing>();

            // Now get with no setup
            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName")
                });

                var local = stack.ClonePreservingOrder();
                var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
                var result = await GetPropStr.ExecuteAsync(parameters);
                Assert.NotNull(result);
                Assert.IsTrue(result.IsSuccessful);

                Assert.AreEqual(1, local.Count);
                var pop = local.Pop();
                Assert.AreEqual(DatumType.String, pop.Type);
                Assert.AreEqual("", pop.Value);
            }

            Assert.Pass();
        }
    }
}