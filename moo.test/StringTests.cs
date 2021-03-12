using System.Collections.Generic;
using moo.common.Models;
using moo.common.Scripting;
using moo.common.Scripting.ForthPrimatives;
using NUnit.Framework;

namespace Tests
{
    public class StringTests
    {
        [Test]
        public void StripLeadingSpace()
        {
            /*
             STRIPLEAD (s -- s)

            Strips leading spaces from the given string.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(" abcd "));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = StripLead.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var res = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, res.Type);
            Assert.AreEqual("abcd ", res.Value);
            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void StripTrailingSpace()
        {
            /*
             STRIPTAIL (s -- s)

             Strips trailing spaces from the given string. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(" abcd "));

            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = StripTail.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var res = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, res.Type);
            Assert.AreEqual(" abcd", res.Value);
            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void SplitNoMatch()
        {
            /*
             SPLIT ( s1 s2 -- s1' s2' )

             Splits string s1 at the first found instance of s2. If there are no matches of s2 in s1, will return s1 and a null string.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("abcdefg"));
            stack.Push(new ForthDatum("x"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = Split.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual(string.Empty, s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("abcdefg", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void SplitOneMatch()
        {
            /*
             SPLIT ( s1 s2 -- s1' s2' )

             Splits string s1 at the first found instance of s2. If there are no matches of s2 in s1, will return s1 and a null string.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("abcdefg"));
            stack.Push(new ForthDatum("d"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = Split.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("efg", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("abc", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void SplitTwoMatches()
        {
            /*
             SPLIT ( s1 s2 -- s1' s2' )

             Splits string s1 at the first found instance of s2. If there are no matches of s2 in s1, will return s1 and a null string.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("abcdefgdhij"));
            stack.Push(new ForthDatum("d"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = Split.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("efgdhij", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("abc", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }


        [Test]
        public void ReverseSplitNoMatch()
        {
            /*
             RSPLIT ( s1 s2 -- s1' s2' )

            Splits a string, as SPLIT, but splits on the last occurence of s2. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("abcdefg"));
            stack.Push(new ForthDatum("x"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = RSplit.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual(string.Empty, s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("abcdefg", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void ReverseSplitOneMatch()
        {
            /*
             RSPLIT ( s1 s2 -- s1' s2' )

            Splits a string, as SPLIT, but splits on the last occurence of s2. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("abcdefg"));
            stack.Push(new ForthDatum("d"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = RSplit.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("efg", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("abc", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void ReverseSplitTwoMatches()
        {
            /*
             RSPLIT ( s1 s2 -- s1' s2' )

            Splits a string, as SPLIT, but splits on the last occurence of s2. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("abcdefgdhij"));
            stack.Push(new ForthDatum("d"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = RSplit.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("hij", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("abcdefg", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void StrCutNormal()
        {
            /*
             STRCUT ( s i -- s1 s2 )

            Cuts string s after its i'th character. For example,
            "Foobar" 3 strcut returns
            "Foo" "bar" If i is zero or greater than the length of s, returns a null string in the first or second position, respectively. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("Foobar"));
            stack.Push(new ForthDatum(3));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = StrCut.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("bar", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("Foo", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void StrCutZero()
        {
            /*
             STRCUT ( s i -- s1 s2 )

            Cuts string s after its i'th character. For example,
            "Foobar" 3 strcut returns
            "Foo" "bar" If i is zero or greater than the length of s, returns a null string in the first or second position, respectively. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("Foobar"));
            stack.Push(new ForthDatum(0));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = StrCut.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("Foobar", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void StrCutTooBig()
        {
            /*
             STRCUT ( s i -- s1 s2 )

            Cuts string s after its i'th character. For example,
            "Foobar" 3 strcut returns
            "Foo" "bar" If i is zero or greater than the length of s, returns a null string in the first or second position, respectively. 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("Foobar"));
            stack.Push(new ForthDatum(int.MaxValue));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = StrCut.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(2, local.Count);
            var s2 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s2.Type);
            Assert.AreEqual("", s2.Value);

            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("Foobar", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        public void Midstr()
        {
            /*
             MIDSTR ( s i1 i2 -- s )

            Returns the substring of i2 characters, starting with character i1. i1 and i2 must both be positive.
            The first character of the string is considered position 1. ie: "testing" 2 3 midstr will return the value "est". 
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum("testing"));
            stack.Push(new ForthDatum(2));
            stack.Push(new ForthDatum(3));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = MidStr.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var s1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.String, s1.Type);
            Assert.AreEqual("est", s1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("dg")]
        [TestCase("dog")]
        [TestCase("doog")]
        [TestCase("dorfg")]
        [TestCase("DG")]
        [TestCase("DOG")]
        [TestCase("DoOg")]
        public void SMatchWildDgMatch(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("d*g"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("dog")]
        [TestCase("dig")]
        [TestCase("dug")]
        public void SMatchCharDgMatch(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("d?g"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("dg")]
        [TestCase("drug")]
        public void SMatchCharDgFail(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("d?g"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(0, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Mr.")]
        [TestCase("Ms.")]
        public void SMatchCharBracketCharMatch(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("M[rs]."));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Ma")]
        [TestCase("Mb")]
        public void SMatchCharBracketRangeMatch(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("M[a-z]"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Moira snores")]
        [TestCase("Chupchup arghs.")]
        public void SMatchCharWordMatch(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("{Moira|Chupchup}*"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Moira' snores")]
        public void SMatchCharWordFail(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum("{Moira|Chupchup}*"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(0, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Foxen tickles Wolfen?")]
        [TestCase("Lynx tickle Wolfen?")]
        [TestCase("Fiera tyckle Wolfen?")]
        [TestCase("Fiero tyckle?")]
        public void SMatchCharWordMatchComplex(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum(@"{Foxen|Lynx|Fier[ao]} *t[iy]ckle*\?"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Sean")]
        [TestCase("Jacob")]
        public void SMatchCharWordNegateMatch(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum(@"{^Foxen|Fiera}"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(1, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }

        [Test]
        [TestCase("Foxen")]
        [TestCase("Fiera")]
        public void SMatchCharWordNegateFail(string test)
        {
            /*
            SMATCH ( s s2 -- i )

            Takes a string s, and a string pattern, s2, to check against.
            Returns true if the string fits the pattern.
            This is case insensitive.
             */
            var stack = new Stack<ForthDatum>();
            stack.Push(new ForthDatum(test));
            stack.Push(new ForthDatum(@"{^Foxen|Fiera}"));
            var local = stack.ClonePreservingOrder();
            var parameters = new ForthPrimativeParameters(null, local, null, null, Dbref.NOT_FOUND, null, null, null, null, default);
            var result = SMatch.Execute(parameters);
            Assert.NotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            Assert.AreEqual(1, local.Count);
            var n1 = local.Pop();
            Assert.AreEqual(ForthDatum.DatumType.Integer, n1.Type);
            Assert.AreEqual(0, n1.Value);

            Assert.AreEqual(0, local.Count);
            Assert.Pass();
        }
    }
}