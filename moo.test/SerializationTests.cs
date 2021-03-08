using moo.common;
using moo.common.Models;
using NUnit.Framework;

namespace Tests
{
    public class SerializationTests
    {
        [Test]
        public void SerializeDeserializeThing()
        {
            var testObj = ThingRepository.Instance.Make<Thing>();
            testObj.externalDescription = "test externalDescription";
            testObj.SetFlag(Thing.Flag.QUELL);
            testObj.name = "test name";
            testObj.owner = new Dbref(456, Dbref.DbrefObjectType.Player);
            testObj.pennies = 789;
            testObj.SetPropertyPathValue("test/deep/string", Property.PropertyType.String, "string");
            testObj.SetPropertyPathValue("test/deep/int", 321);
            testObj.SetPropertyPathValue("test/deep/dbref", new Dbref(2468, Dbref.DbrefObjectType.Garbage));
            testObj.SetPropertyPathValue("test/deep/float", 12.34F);
            var serialized = testObj.Serialize();
            Assert.NotNull(serialized);

            var deserialized = Thing.Deserialize<Thing>(serialized);
            Assert.NotNull(deserialized);
            Assert.AreEqual(testObj.externalDescription, deserialized.externalDescription);
            Assert.AreEqual(testObj.flags, deserialized.flags);
            Assert.AreEqual(testObj.id, deserialized.id);
            Assert.AreEqual(testObj.Location, deserialized.Location);
            Assert.AreEqual(testObj.Owner, deserialized.Owner);
            Assert.AreEqual(testObj.pennies, deserialized.pennies);
            Assert.AreEqual(testObj.properties, deserialized.properties);
            Assert.AreEqual(testObj.templates, deserialized.templates);
            Assert.AreEqual(testObj.Type, deserialized.Type);

            var reserialized = deserialized.Serialize();
            Assert.NotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
            Assert.Pass();
        }

    }
}