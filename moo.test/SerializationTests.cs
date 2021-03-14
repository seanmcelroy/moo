using System;
using System.Collections.Generic;
using System.Linq;
using moo.common;
using moo.common.Models;
using NUnit.Framework;

namespace Tests
{
    public class SerializationTests
    {
        [Test]
        public void ReserializeFloat()
        {
            var testFloat = 123.456F;
            var serialized = Thing.Serialize(testFloat);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testFloat, deserialized);
        }

        [Test]
        public void ReserializeFloatNull()
        {
            float? testFloat = null;
            var serialized = Thing.Serialize(testFloat);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(testFloat, deserialized);
        }

        [Test]
        public void ReserializeInt()
        {
            var testInt = 123;
            var serialized = Thing.Serialize(testInt);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testInt, deserialized);
        }

        [Test]
        public void ReserializeIntNull()
        {
            float? testInt = null;
            var serialized = Thing.Serialize(testInt);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(testInt, deserialized);
        }

        [Test]
        public void ReserializeUInt16()
        {
            ushort testUInt16 = 123;
            var serialized = Thing.Serialize(testUInt16);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testUInt16, deserialized);
        }

        [Test]
        public void ReserializeUInt16Null()
        {
            ushort? testUInt16 = null;
            var serialized = Thing.Serialize(testUInt16);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(testUInt16, deserialized);
        }

        [Test]
        public void ReserializeDateTime()
        {
            DateTime? testDateTime = DateTime.UtcNow;
            var serialized = Thing.Serialize(testDateTime);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testDateTime, deserialized);
        }

        [Test]
        public void ReserializeDateTimeNull()
        {
            DateTime? testDateTime = null;
            var serialized = Thing.Serialize(testDateTime);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(testDateTime, deserialized);
        }

        [Test]
        public void ReserializeDbref()
        {
            Dbref? test = Dbref.GOD;
            var serialized = Thing.Serialize(test);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(test, deserialized);
        }

        [Test]
        public void ReserializeDbrefNull()
        {
            Dbref? test = null;
            var serialized = Thing.Serialize(test);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(test, deserialized);
        }

        [Test]
        public void ReserializeArray()
        {
            var testArray = new string[] { "one", "two", "three" };
            var serialized = Thing.Serialize(testArray);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testArray, deserialized);
        }

        [Test]
        public void ReserializeArrayNull()
        {
            string[]? testArray = null;
            var serialized = Thing.Serialize(testArray);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(testArray, deserialized);
        }

        [Test]
        public void ReserializeDictionary()
        {
            var testDict = new Dictionary<string, object?> {
                 { "string", "STRING" }
            };
            var serialized = Thing.Serialize(testDict);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testDict, deserialized);
        }

        [Test]
        public void ReserializeDictionaryNull()
        {
            Dictionary<string, object?>? testDict = null;
            var serialized = Thing.Serialize(testDict);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
        }

        [Test]
        public void ReserializeString()
        {
            var testString = "test-string < & ";
            var serialized = Thing.Serialize(testString);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testString, deserialized);
        }

        [Test]
        public void ReserializeStringNull()
        {
            string? testString = null;
            var serialized = Thing.Serialize(testString);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.Null(deserialized);
            Assert.AreEqual(testString, deserialized);
        }

        [Test]
        public void ReserializeStringEmpty()
        {
            string? testString = string.Empty;
            var serialized = Thing.Serialize(testString);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(testString, deserialized);
        }

        [Test]
        public void ReserializePropertyDirectory0Level()
        {
            var propdir = new PropertyDirectory {
                {"prop1", "STRING"},
                {"prop2", 123},
            };
            var serialized = Thing.Serialize(propdir);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized).ToArray();
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(propdir, deserialized);
        }

        [Test]
        public void ReserializePropertyDirectory1Level()
        {
            var propdir = new PropertyDirectory
            {
                { "level1/prop1", "STRING" },
                { "level1/prop2", 123 }
            };
            Assert.AreEqual(1, propdir.Count);

            var serialized = Thing.Serialize(propdir);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized).ToArray();
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.IsInstanceOf<PropertyDirectory>(deserialized);
            Assert.AreEqual(propdir.ElementAt(0), ((PropertyDirectory)deserialized).ElementAt(0));
            Assert.AreEqual(propdir, deserialized);
        }

        [Test]
        public void SerializeDeserializeThing()
        {
            var testObj = ThingRepository.Instance.Make<Thing>();
            testObj.externalDescription = "test externalDescription";
            testObj.SetFlag(Thing.Flag.QUELL);
            testObj.name = "test name";
            testObj.owner = new Dbref(456, Dbref.DbrefObjectType.Player);
            testObj.pennies = 789;
            testObj.SetPropertyPathValue("test/deep/string", Property.PropertyType.String, "STRING TEST < WOO >");
            testObj.SetPropertyPathValue("test/deep/int", 321);
            testObj.SetPropertyPathValue("test/deep/dbref", new Dbref(2468, Dbref.DbrefObjectType.Garbage));
            testObj.SetPropertyPathValue("test/deep/float", 12.34F);
            var serialized = testObj.Serialize();
            Assert.NotNull(serialized);

            var deserialized = Thing.Deserialize<Thing>(serialized);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.properties);
            Assert.AreEqual(testObj.properties.Count, deserialized.properties.Count);
            Assert.AreEqual(testObj.properties, deserialized.properties);
            Assert.AreEqual(testObj.externalDescription, deserialized.externalDescription);
            Assert.AreEqual(testObj.flags, deserialized.flags);
            Assert.AreEqual(testObj.id, deserialized.id);
            Assert.AreEqual(testObj.Location, deserialized.Location);
            Assert.AreEqual(testObj.Owner, deserialized.Owner);
            Assert.AreEqual(testObj.pennies, deserialized.pennies);
            Assert.AreEqual(testObj.templates, deserialized.templates);
            Assert.AreEqual(testObj.Type, deserialized.Type);

            var reserialized = deserialized.Serialize();
            Assert.NotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
        }
    }
}