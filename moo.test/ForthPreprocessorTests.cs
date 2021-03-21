using System.Threading;
using System.Threading.Tasks;
using moo.common.Models;
using moo.common.Scripting;
using NUnit.Framework;

namespace Tests
{
    public class ForthPreprocessorTests
    {
        [Test]
        public async Task SimpleIfndef()
        {
            var script = new Script
            {
                programText = "$ifndef FOO<3\nBAR\n$enddef"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("BAR\r\n", prep.ProcessedProgram);
        }

        [Test]
        public async Task SimpleDefReplacement()
        {
            var script = new Script
            {
                programText = "$def VARIAB 3\nVARIAB push"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("3 push\r\n", prep.ProcessedProgram);
        }

        [Test]
        public async Task SimpleDefIfdefTest()
        {
            var script = new Script
            {
                programText = "$def FLAG\n$ifdef FLAG\n3 push\n$enddef"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("3 push\r\n", prep.ProcessedProgram);
        }

        [Test]
        public async Task SimpleDefIfdefElseTest()
        {
            var script = new Script
            {
                programText = "$def FLAG\n$ifdef FLAG\n3 3\n$else\n2 2\n$enddef"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("3 3\r\n", prep.ProcessedProgram);
        }

        [Test]
        public async Task SimpleDefIfndefElseTest()
        {
            var script = new Script
            {
                programText = "$def FLAG\n$ifndef FLAG\n3 3\n$else\n2 2\n$enddef"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("2 2\r\n", prep.ProcessedProgram);
        }

        [Test]
        public async Task NestedIfDefs()
        {
            var script = new Script
            {
                programText = "$ifdef __version>Muck2.2fb3.5\n    $def envprop .envprop\n    $endif\n    $define ploc\n        $ifdef proplocs\n            .proploc\n        $else\n            owner\n        $endif\n    $enddef"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("", prep.ProcessedProgram);
        }

        [Test]
        public async Task NestedIfDefs2()
        {
            var script = new Script
            {
                programText = "$def FLAG\r\n$ifdef FLAG\r\n$ifdef FLAG2\r\n2 2\r\n$else\r\n3 3\r\r$endif\r\n$else\r\n4 4\r\n$endif"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("3 3", prep.ProcessedProgram?.TrimEnd(new char[] { '\r', '\n' }));
        }

        [Test]
        public async Task NestedIfDefs3()
        {
            var script = new Script
            {
                programText = "$def FLAG\r\n$ifdef FLAG\r\n$ifdef FLAG\r\n2 2\r\n$else\r\n3 3\r\r$endif\r\n$else\r\n4 4\r\n$endif"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("2 2", prep.ProcessedProgram?.TrimEnd(new char[] { '\r', '\n' }));
        }

        [Test]
        public async Task NestedIfDefs4()
        {
            var script = new Script
            {
                programText = "$def FLAG\r\n$ifdef FLAG2\r\n$ifdef FLAG2\r\n2 2\r\n$else\r\n3 3\r\r$endif\r\n$else\r\n4 4\r\n$endif"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("4 4", prep.ProcessedProgram?.TrimEnd(new char[] { '\r', '\n' }));
        }

        [Test]
        public async Task NestedIfDefs5()
        {
            var script = new Script
            {
                programText = "$def FLAG\r\n$ifdef FLAG2\r\n$ifdef FLAG2\r\n2 2\r\n$else\r\n3 3\r\r$endif\r\n$endif\r\n5 5\r\n"
            };

            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("5 5", prep.ProcessedProgram?.TrimEnd(new char[] { '\r', '\n' }));
        }

        [Test]
        public async Task DefineExpansion()
        {
            var programText = "$def stripspaces strip\n: getprepend (playerdbref -- string)\n     \"_whisp/prepend\" getpropstr dup not if\n        me @ \"_whisp/prepend\" \"W>>>\" setprop \n     else stripspaces \" \" strcat\n     then\n;";
            var prep = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.NotNull(prep.ProcessedProgram);
            Assert.IsFalse(prep.ProcessedProgram!.Contains(" strip ", System.StringComparison.OrdinalIgnoreCase));
        }
    }
}