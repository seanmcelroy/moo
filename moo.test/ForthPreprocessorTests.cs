using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    public class ForthPreprocessorTests
    {
        [Test]
        public async Task SimpleIfndef()
        {
            var script = new Script();
            script.programText = "$ifndef FOO<3\nBAR\n$enddef";

            var prep = await ForthPreprocessor.Preprocess(null, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("BAR\r\n", prep.ProcessedProgram);

            Assert.Pass();
        }

        [Test]
        public async Task SimpleDefReplacement()
        {
            var script = new Script();
            script.programText = "$def VARIAB 3\nVARIAB push";

            var prep = await ForthPreprocessor.Preprocess(null, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("3 push\r\n", prep.ProcessedProgram);

            Assert.Pass();
        }

        [Test]
        public async Task SimpleDefIfdefTest()
        {
            var script = new Script();
            script.programText = "$def FLAG\n$ifdef FLAG\n3 push\n$enddef";

            var prep = await ForthPreprocessor.Preprocess(null, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("3 push\r\n", prep.ProcessedProgram);

            Assert.Pass();
        }

        [Test]
        public async Task SimpleDefIfndefTest()
        {
            var script = new Script();
            script.programText = "$def FLAG\n$ifndef FLAG\n3 3\n$else\n2 2\n$enddef";

            var prep = await ForthPreprocessor.Preprocess(null, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("2 2\r\n", prep.ProcessedProgram);

            Assert.Pass();
        }

        [Test]
        public async Task NestedIfDefs()
        {
            var script = new Script();
            script.programText = "$ifdef __version>Muck2.2fb3.5\n    $def envprop .envprop\n    $endif\n    $define ploc\n        $ifdef proplocs\n            .proploc\n        $else\n            owner\n        $endif\n    $enddef";

            var prep = await ForthPreprocessor.Preprocess(null, script, script.programText, CancellationToken.None);
            Assert.IsTrue(prep.IsSuccessful);
            Assert.AreEqual("", prep.ProcessedProgram);

            Assert.Pass();
        }
    }
}