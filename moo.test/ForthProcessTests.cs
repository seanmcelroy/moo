using System.Threading.Tasks;
using moo.common.Models;
using moo.common.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace moo.Test
{
    [TestClass]
    public class ForthProcessTests
    {
        [TestMethod]
        public async Task UnknownIdentifierReturnsCleanError()
        {
            // An identifier that isn't a word, primitive, built-in, or variable
            // used to be silently pushed onto the stack as `DatumType.Unknown`,
            // so the program completed "successfully" with garbage on the
            // stack. The fix rejects Unknown datums in the runtime dispatch
            // path with a SYNTAX_ERROR. (Was originally proposed as a test for
            // RunWordAsync's Single→FirstOrDefault fix; that path is now also
            // safer but is unreachable for truly missing words because
            // HasWord() filters them out before RunWordAsync is called.)
            const string program = ": main does_not_exist ;";
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(program);
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public async Task RunAsyncAcceptsIntegerArg()
        {
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(": main ;", 7);
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(7, stack.Pop().Value);
        }

        [TestMethod]
        public async Task RunAsyncAcceptsDbrefArg()
        {
            // Targets #25 — pre-fix RunAsync only handled args[0] as a string.
            var (result, stack) = await ExecutionTestHelpers.RunProgramAsync(": main ;", new Dbref("#42"));
            Assert.IsTrue(result.IsSuccessful, result.Reason);
            Assert.AreEqual(1, stack.Count);
            var top = stack.Pop();
            Assert.AreEqual(ForthDatum.DatumType.DbRef, top.Type);
            Assert.AreEqual(42, top.UnwrapDbref().ToInt32());
        }

        [TestMethod]
        public async Task RunAsyncRejectsUnsupportedArgType()
        {
            // Targets the new switch in #25 — anything other than the supported
            // primitive types should be cleanly rejected.
            var (result, _) = await ExecutionTestHelpers.RunProgramAsync(": main ;", new System.DateTime(2026, 1, 1));
            Assert.IsFalse(result.IsSuccessful);
        }

        [TestMethod]
        public void SetProgramLocalVariableLowercasesKey()
        {
            // Targets §3.E.2. Pre-fix this had a TryAdd-then-write race AND
            // stored the original-case key; post-fix it is a single atomic
            // indexer write AND lowercases the key for consistency with the
            // rest of the variable plumbing.
            var process = new ForthProcess(Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, "test", 0);
            process.SetProgramLocalVariable("FooBar", new ForthVariable("hello"));
            var dict = process.GetProgramLocalVariables();
            Assert.IsTrue(dict.ContainsKey("foobar"));
            Assert.IsFalse(dict.ContainsKey("FooBar"));
        }
    }
}
