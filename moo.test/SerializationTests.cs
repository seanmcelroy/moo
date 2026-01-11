using System;
using System.Collections.Generic;
using System.Linq;
using moo.common;
using moo.common.Database;
using moo.common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class SerializationTests : TestBase
    {
        [TestMethod]
        public void ReserializeFloat()
        {
            var testFloat = 123.456F;
            var serialized = Thing.Serialize(testFloat);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testFloat, deserialized);
        }

        [TestMethod]
        public void ReserializeFloatNull()
        {
            float? testFloat = null;
            var serialized = Thing.Serialize(testFloat);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(testFloat, deserialized);
        }

        [TestMethod]
        public void ReserializeInt()
        {
            var testInt = 123;
            var serialized = Thing.Serialize(testInt);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testInt, deserialized);
        }

        [TestMethod]
        public void ReserializeIntNull()
        {
            float? testInt = null;
            var serialized = Thing.Serialize(testInt);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(testInt, deserialized);
        }

        [TestMethod]
        public void ReserializeUInt16()
        {
            ushort testUInt16 = 123;
            var serialized = Thing.Serialize(testUInt16);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testUInt16, deserialized);
        }

        [TestMethod]
        public void ReserializeUInt16Null()
        {
            ushort? testUInt16 = null;
            var serialized = Thing.Serialize(testUInt16);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(testUInt16, deserialized);
        }

        [TestMethod]
        public void ReserializeDateTime()
        {
            DateTime? testDateTime = DateTime.UtcNow;
            var serialized = Thing.Serialize(testDateTime);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testDateTime, deserialized);
        }

        [TestMethod]
        public void ReserializeDateTimeNull()
        {
            DateTime? testDateTime = null;
            var serialized = Thing.Serialize(testDateTime);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(testDateTime, deserialized);
        }

        [TestMethod]
        public void ReserializeDbref()
        {
            Dbref? test = Dbref.GOD;
            var serialized = Thing.Serialize(test);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(test, deserialized);
        }

        [TestMethod]
        public void ReserializeDbrefNull()
        {
            Dbref? test = null;
            var serialized = Thing.Serialize(test);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(test, deserialized);
        }

        [TestMethod]
        public void ReserializeArrayString()
        {
            string[] testArray = ["one", "two", "three"];
            var serialized = Thing.Serialize(testArray);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.IsInstanceOfType<List<object>>(deserialized);
            var deserializedToColl = (System.Collections.ICollection?)deserialized;
            CollectionAssert.AreEqual(testArray, deserializedToColl);
        }

        [TestMethod]
        public void ReserializeArrayDbref()
        {
            var testArray = new Dbref[] { Dbref.Parse("#156E") };
            var serialized = Thing.Serialize(testArray);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.IsInstanceOfType<List<object>>(deserialized);
            var deserializedToColl = (System.Collections.ICollection?)deserialized;
            CollectionAssert.AreEqual(testArray, deserializedToColl);
        }

        [TestMethod]
        public void ReserializeArrayNull()
        {
            string[]? testArray = null;
            var serialized = Thing.Serialize(testArray);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(testArray, deserialized);
        }

        [TestMethod]
        public void ReserializeConcurrentDbrefSet()
        {
            var cds = new ConcurrentDbrefSet(new[] { Dbref.Parse("#123E") });
            var serialized = Thing.Serialize(cds);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(cds, deserialized);
        }

        [TestMethod]
        public void ReserializeConcurrentDbrefSetNull()
        {
            ConcurrentDbrefSet? test = null;
            var serialized = Thing.Serialize(test);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(test, deserialized);
        }

        [TestMethod]
        public void ReserializeDictionary()
        {
            var testDict = new Dictionary<string, object?> {
                 { "string", "STRING" }
            };
            var serialized = Thing.Serialize(testDict);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.IsInstanceOfType<Dictionary<string, object>>(deserialized);
            var deserializedToColl = (System.Collections.ICollection?)deserialized;
            CollectionAssert.AreEqual(testDict, deserializedToColl);
        }

        [TestMethod]
        public void ReserializeDictionaryNull()
        {
            Dictionary<string, object?>? testDict = null;
            var serialized = Thing.Serialize(testDict);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
        }

        [TestMethod]
        public void ReserializeString()
        {
            var testString = "test-string < & ";
            var serialized = Thing.Serialize(testString);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testString, deserialized);
        }

        [TestMethod]
        public void ReserializeStringNull()
        {
            string? testString = null;
            var serialized = Thing.Serialize(testString);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNull(deserialized);
            Assert.AreEqual(testString, deserialized);
        }

        [TestMethod]
        public void ReserializeStringEmpty()
        {
            string? testString = string.Empty;
            var serialized = Thing.Serialize(testString);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testString, deserialized);
        }

        [TestMethod]
        public void ReserializePropertyDirectory0Level()
        {
            var propdir = new PropertyDirectory {
                {"prop1", "STRING"},
                {"prop2", 123},
            };
            var serialized = Thing.Serialize(propdir);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized).ToArray();
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(propdir, deserialized);
        }

        [TestMethod]
        public void ReserializePropertyDirectory1Level()
        {
            var propdir = new PropertyDirectory
            {
                { "level1/prop1", "STRING" },
                { "level1/prop2", 123 }
            };
            Assert.AreEqual(1, propdir.Count);

            var serialized = Thing.Serialize(propdir);
            Assert.IsNotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized).ToArray();
            Assert.IsNotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.IsNotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.IsNotNull(deserialized);
            Assert.IsInstanceOfType(deserialized, typeof(PropertyDirectory));
            Assert.AreEqual(propdir.ElementAt(0), ((PropertyDirectory)deserialized!).ElementAt(0));
            Assert.AreEqual(propdir, deserialized);
        }

        [TestMethod]
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
            Assert.IsNotNull(serialized);

            var deserialized = Thing.Deserialize<Thing>(serialized);
            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized!.properties);
            Assert.AreEqual(testObj.properties.Count, deserialized.properties.Count);
            Assert.AreEqual(testObj.properties, deserialized.properties);
            Assert.AreEqual(testObj.externalDescription, deserialized.externalDescription);
            CollectionAssert.AreEqual(testObj.flags, deserialized.flags);
            Assert.AreEqual(testObj.id, deserialized.id);
            Assert.AreEqual(testObj.Location, deserialized.Location);
            Assert.AreEqual(testObj.Owner, deserialized.Owner);
            Assert.AreEqual(testObj.pennies, deserialized.pennies);
            CollectionAssert.AreEqual(testObj.templates, deserialized.templates);
            Assert.AreEqual(testObj.Type, deserialized.Type);

            var reserialized = deserialized.Serialize();
            Assert.IsNotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
        }

        [TestMethod]
        public void DeserializeContentsConcurrentDbrefSet()
        {
            var json = "{\"properties\":{\"test\":{\"Name\":\"test\",\"directory\":{\"deep\":{\"Name\":\"deep\",\"directory\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}},\"value\":null,\"Type\":5,\"Value\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}}}},\"value\":null,\"Type\":5,\"Value\":{\"deep\":{\"Name\":\"deep\",\"directory\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}},\"value\":null,\"Type\":5,\"Value\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}}}}}},\"aliases\":[\"test\"],\"contents\":[\"#135T\"],\"externalDescription\":\"test externalDescription\",\"flags\":[76],\"id\":\"#0E\",\"linkTargets\":[\"#246R\"],\"location\":\"#0\",\"name\":\"test name\",\"owner\":\"#456P\",\"pennies\":0,\"templates\":[]}";
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Exit>(json, new Newtonsoft.Json.JsonConverter[] {
                new ConcurrentDbrefSetSerializer(),
                new DbrefSerializer()
            });
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.contents.Count);
        }

        [TestMethod]
        public void SerializeDeserializeExit()
        {
            var testExit = ThingRepository.Instance.Make<Exit>();
            testExit.aliases.Add("test");
            testExit.SetLinkTargets(new Dbref[] { new Dbref(246, Dbref.DbrefObjectType.Room) });
            testExit.contents.TryAdd(new Dbref(135, Dbref.DbrefObjectType.Thing));
            testExit.externalDescription = "test externalDescription";
            testExit.SetFlag(Thing.Flag.LINK_OK);
            testExit.name = "test name";
            testExit.owner = new Dbref(456, Dbref.DbrefObjectType.Player);
            testExit.SetPropertyPathValue("test/deep/string", Property.PropertyType.String, "STRING TEST < WOO >");
            testExit.SetPropertyPathValue("test/deep/int", 321);
            testExit.SetPropertyPathValue("test/deep/dbref", new Dbref(2468, Dbref.DbrefObjectType.Garbage));
            testExit.SetPropertyPathValue("test/deep/float", 12.34F);
            var serialized = testExit.Serialize();
            Assert.IsNotNull(serialized);

            var deserialized = Thing.Deserialize<Exit>(serialized);
            Assert.IsNotNull(deserialized);
            Assert.IsInstanceOfType<Exit>(deserialized);
            System.Collections.ICollection testExitAliasesToColl = testExit.aliases.ToArray();
            System.Collections.ICollection deserializedToColl = deserialized.aliases.ToArray();
            CollectionAssert.AreEqual(testExitAliasesToColl, deserializedToColl);
            Assert.AreEqual(testExit.contents.Count, deserialized.contents.Count);
            Assert.AreEqual(testExit.contents, deserialized.contents);
            Assert.AreEqual(testExit.properties.Count, deserialized.properties.Count);
            Assert.AreEqual(testExit.properties, deserialized.properties);
            Assert.AreEqual(testExit.externalDescription, deserialized.externalDescription);
            CollectionAssert.AreEqual(testExit.flags, deserialized.flags);
            Assert.AreEqual(testExit.id, deserialized.id);
            Assert.AreEqual(testExit.Location, deserialized.Location);
            Assert.AreEqual(testExit.Owner, deserialized.Owner);
            Assert.AreEqual(testExit.pennies, deserialized.pennies);
            CollectionAssert.AreEqual(testExit.templates, deserialized.templates);
            Assert.AreEqual(testExit.Type, deserialized.Type);

            var reserialized = deserialized.Serialize();
            Assert.IsNotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
        }

        [TestMethod]
        public void SerializeDeserializeHumanPlayer()
        {
            var player = ThingRepository.Instance.Make<HumanPlayer>();
            player.id = new Dbref(1, Dbref.DbrefObjectType.Player);
            Assert.IsTrue(player.SetPassword("secret"));
            Assert.IsFalse(player.SetPassword(" "));

            Assert.IsFalse(player.CheckPassword("secret "));
            Assert.IsFalse(player.CheckPassword("SECRET"));
            Assert.IsFalse(player.CheckPassword(" secret"));
            Assert.IsFalse(player.CheckPassword("hunter2"));
            Assert.IsFalse(player.CheckPassword(""));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.IsFalse(player.CheckPassword(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.IsTrue(player.CheckPassword("secret"));

            var serialized = player.Serialize();
            Assert.IsNotNull(serialized);

            var deserialized = Thing.Deserialize<HumanPlayer>(serialized);
            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized!.properties);
            Assert.AreEqual(player.properties.Count, deserialized.properties.Count);
            Assert.AreEqual(player.properties, deserialized.properties);

            Assert.IsFalse(deserialized.CheckPassword("hunter2"));
            Assert.IsTrue(deserialized.CheckPassword("secret"));

            var reserialized = deserialized.Serialize();
            Assert.IsNotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
        }
    }
}