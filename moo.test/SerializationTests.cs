using System;
using System.Collections.Generic;
using System.Linq;
using moo.common;
using moo.common.Database;
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
        public void ReserializeArrayString()
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
        public void ReserializeArrayDbref()
        {
            var testArray = new Dbref[] { Dbref.Parse("#156E") };
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
        public void ReserializeConcurrentDbrefSet()
        {
            var cds = new ConcurrentDbrefSet(new[] { Dbref.Parse("#123E") });
            var serialized = Thing.Serialize(cds);
            Assert.NotNull(serialized);
            var deserializedResult = Thing.DeserializePart(serialized);
            Assert.NotNull(deserializedResult);
            var deserializedResultArray = deserializedResult.ToArray();
            Assert.NotNull(deserializedResultArray);
            Assert.AreEqual(1, deserializedResultArray.Length);
            Assert.AreEqual(string.Empty, deserializedResultArray[0].Item2);
            var deserialized = deserializedResultArray[0].Item1;
            Assert.NotNull(deserialized);
            Assert.AreEqual(cds, deserialized);
        }

        [Test]
        public void ReserializeConcurrentDbrefSetNull()
        {
            ConcurrentDbrefSet? test = null;
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

        [Test]
        public void DeserializeContentsConcurrentDbrefSet()
        {
            var json = "{\"properties\":{\"test\":{\"Name\":\"test\",\"directory\":{\"deep\":{\"Name\":\"deep\",\"directory\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}},\"value\":null,\"Type\":5,\"Value\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}}}},\"value\":null,\"Type\":5,\"Value\":{\"deep\":{\"Name\":\"deep\",\"directory\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}},\"value\":null,\"Type\":5,\"Value\":{\"string\":{\"Name\":\"string\",\"directory\":null,\"value\":\"STRING TEST < WOO >\",\"Type\":1,\"Value\":\"STRING TEST < WOO >\"},\"int\":{\"Name\":\"int\",\"directory\":null,\"value\":321.0,\"Type\":4,\"Value\":321.0},\"dbref\":{\"Name\":\"dbref\",\"directory\":null,\"value\":\"#2468G\",\"Type\":3,\"Value\":\"#2468G\"},\"float\":{\"Name\":\"float\",\"directory\":null,\"value\":12.34,\"Type\":4,\"Value\":12.34}}}}}},\"aliases\":[\"test\"],\"contents\":[\"#135T\"],\"externalDescription\":\"test externalDescription\",\"flags\":[76],\"id\":\"#0E\",\"linkTargets\":[\"#246R\"],\"location\":\"#0\",\"name\":\"test name\",\"owner\":\"#456P\",\"pennies\":0,\"templates\":[]}";
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Exit>(json, new Newtonsoft.Json.JsonConverter[] {
                new ConcurrentDbrefSetSerializer(),
                new DbrefSerializer()
            });
            Assert.NotNull(result);
            Assert.NotNull(result.contents);
            Assert.AreEqual(1, result.contents.Count);
        }

        [Test]
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
            Assert.NotNull(serialized);

            var deserialized = Thing.Deserialize<Exit>(serialized);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.properties);
            Assert.AreEqual(testExit.aliases, deserialized.aliases);
            Assert.NotNull(deserialized.contents);
            Assert.AreEqual(testExit.contents.Count, deserialized.contents.Count);
            Assert.AreEqual(testExit.contents, deserialized.contents);
            Assert.AreEqual(testExit.properties.Count, deserialized.properties.Count);
            Assert.AreEqual(testExit.properties, deserialized.properties);
            Assert.AreEqual(testExit.externalDescription, deserialized.externalDescription);
            Assert.AreEqual(testExit.flags, deserialized.flags);
            Assert.AreEqual(testExit.id, deserialized.id);
            Assert.AreEqual(testExit.Location, deserialized.Location);
            Assert.AreEqual(testExit.Owner, deserialized.Owner);
            Assert.AreEqual(testExit.pennies, deserialized.pennies);
            Assert.AreEqual(testExit.templates, deserialized.templates);
            Assert.AreEqual(testExit.Type, deserialized.Type);

            var reserialized = deserialized.Serialize();
            Assert.NotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
        }

        [Test]
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
            Assert.IsFalse(player.CheckPassword(null));
            Assert.IsTrue(player.CheckPassword("secret"));

            var serialized = player.Serialize();
            Assert.NotNull(serialized);

            var deserialized = Thing.Deserialize<HumanPlayer>(serialized);
            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.properties);
            Assert.AreEqual(player.properties.Count, deserialized.properties.Count);
            Assert.AreEqual(player.properties, deserialized.properties);

            Assert.IsFalse(deserialized.CheckPassword("hunter2"));
            Assert.IsTrue(deserialized.CheckPassword("secret"));

            var reserialized = deserialized.Serialize();
            Assert.NotNull(reserialized);

            Assert.AreEqual(serialized, reserialized);
        }
    }
}