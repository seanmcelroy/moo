using System.Collections.Generic;
using System.Threading.Tasks;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;
using static moo.common.Models.Property;

namespace Tests
{
    public class AddPropTest
    {
        [Test]
        public async Task AddPropNormalString()
        {
            /*
            ADDPROP ( d s1 s2 i -- )

            Sets property associated with s1 in object d. Note that if s2 is null "", then i will be used.
            Otherwise, s2 is always used. All four parameters must be on the stack; none may be omitted.
            If the effective user of the program does not control the object in question and the property begins with an underscore `_',
            the property cannot be changed. The same goes for properties beginning with a dot `.' which cannot be read without permission.
             */

            var testObj = ThingRepository.Instance.Make<Thing>();

            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(testObj.id));
            stack.Push(new ForthDatum("propName"));
            stack.Push(new ForthDatum("propValue"));
            stack.Push(new ForthDatum(123));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);

            var result = await AddProp.ExecuteAsync(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.NotNull(testObj.properties);
            Assert.AreEqual(1, testObj.properties.Count);
            Assert.IsTrue(testObj.properties.ContainsKey("propName"));
            var prop = testObj.properties["propName"];
            Assert.NotNull(prop);
            Assert.AreEqual("propName", prop.Name);
            Assert.AreEqual(PropertyType.String, prop.Type);
            Assert.AreEqual("propValue", prop.Value);
        }

        [Test]
        public async Task AddPropNormalInteger()
        {
            /*
            ADDPROP ( d s1 s2 i -- )

            Sets property associated with s1 in object d. Note that if s2 is null "", then i will be used.
            Otherwise, s2 is always used. All four parameters must be on the stack; none may be omitted.
            If the effective user of the program does not control the object in question and the property begins with an underscore `_',
            the property cannot be changed. The same goes for properties beginning with a dot `.' which cannot be read without permission.
             */

            var testObj = ThingRepository.Instance.Make<Thing>();

            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(testObj.id));
            stack.Push(new ForthDatum("propName"));
            stack.Push(new ForthDatum(""));
            stack.Push(new ForthDatum(123));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);

            var result = await AddProp.ExecuteAsync(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful, result.Reason);

            Assert.NotNull(testObj.properties);
            Assert.AreEqual(1, testObj.properties.Count);
            Assert.IsTrue(testObj.properties.ContainsKey("propName"));
            var prop = testObj.properties["propName"];
            Assert.NotNull(prop);
            Assert.AreEqual("propName", prop.Name);
            Assert.AreEqual(PropertyType.Integer, prop.Type);
            Assert.AreEqual(123, prop.Value);
        }

        [Test]
        public async Task AddPropOverwriteString()
        {
            /*
            ADDPROP ( d s1 s2 i -- )

            Sets property associated with s1 in object d. Note that if s2 is null "", then i will be used.
            Otherwise, s2 is always used. All four parameters must be on the stack; none may be omitted.
            If the effective user of the program does not control the object in question and the property begins with an underscore `_',
            the property cannot be changed. The same goes for properties beginning with a dot `.' which cannot be read without permission.
             */

            var testObj = ThingRepository.Instance.Make<Thing>();

            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName"),
                    new ForthDatum("propValue1"),
                    new ForthDatum(123)
                });
                var parameters = new ForthPrimativeParameters(null, stack.ClonePreservingOrder(), null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null, default);
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

            {
                var stack = new Stack<ForthDatum>(new[]{
                    new ForthDatum(testObj.id),
                    new ForthDatum("propName"),
                    new ForthDatum("propValue2"),
                    new ForthDatum(123)
                });
                var parameters = new ForthPrimativeParameters(null, stack.ClonePreservingOrder(), null, Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, null, null, null, null, null,  default);
                var result2 = await AddProp.ExecuteAsync(parameters);
                Assert.NotNull(result2);
                Assert.IsTrue(result2.IsSuccessful);

                Assert.NotNull(testObj.properties);
                Assert.AreEqual(1, testObj.properties.Count);
                Assert.IsTrue(testObj.properties.ContainsKey("propName"));
                var prop = testObj.properties["propName"];
                Assert.NotNull(prop);
                Assert.AreEqual("propName", prop.Name);
                Assert.AreEqual(PropertyType.String, prop.Type);
                Assert.AreEqual("propValue2", prop.Value);
            }
        }
    }
}